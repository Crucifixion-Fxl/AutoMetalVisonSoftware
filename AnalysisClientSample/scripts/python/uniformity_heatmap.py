#!/usr/bin/env python3
from __future__ import annotations

import argparse
from pathlib import Path

import cv2
import matplotlib as mpl
import matplotlib.pyplot as plt
import numpy as np


def _apply_times_new_roman_style() -> None:
    mpl.rcParams["font.family"] = "serif"
    mpl.rcParams["font.serif"] = ["Times New Roman", "Times", "DejaVu Serif"]
    mpl.rcParams["axes.unicode_minus"] = False


def read_gray_image(path: str | Path) -> np.ndarray:
    p = Path(path)
    if not p.exists():
        raise FileNotFoundError(f"图像不存在: {p}")
    gray = cv2.imread(str(p), cv2.IMREAD_GRAYSCALE)
    if gray is None:
        raise ValueError(f"图像读取失败: {p}")
    return gray


def read_coating_mask_non_black(mask_path: str | Path) -> np.ndarray:
    mask_gray = read_gray_image(mask_path)
    # 业务口径：非纯黑色区域参与均匀性统计
    return mask_gray > 0


def estimate_background_by_gaussian(filled_image: np.ndarray, ksize: int, sigma: float) -> np.ndarray:
    if ksize <= 0 or ksize % 2 == 0:
        raise ValueError("gaussian_ksize 必须是正奇数。")
    return cv2.GaussianBlur(filled_image, (ksize, ksize), sigmaX=sigma, sigmaY=sigma)


def evaluate_uniformity_with_mask(
    image_gray: np.ndarray,
    coating_mask: np.ndarray,
    gaussian_ksize: int,
    gaussian_sigma: float,
    apply_illumination_correction: bool,
):
    if image_gray.shape != coating_mask.shape:
        raise ValueError(f"原图与掩膜尺寸不一致: image={image_gray.shape}, mask={coating_mask.shape}")

    coating_pixels = image_gray[coating_mask].astype(np.float64)
    coating_pixel_count = int(coating_pixels.size)
    if coating_pixel_count == 0:
        corrected = image_gray.astype(np.float64).copy()
        return float("nan"), float("nan"), float("nan"), corrected

    mu_mask_raw = float(coating_pixels.mean())
    image_float = image_gray.astype(np.float64)
    corrected = image_float.copy()

    if apply_illumination_correction:
        filled = image_float.copy()
        filled[~coating_mask] = mu_mask_raw
        background = estimate_background_by_gaussian(filled, gaussian_ksize, gaussian_sigma)
        corrected[coating_mask] = image_float[coating_mask] - background[coating_mask] + mu_mask_raw
        corrected = np.clip(corrected, 0.0, 255.0)

    corrected_pixels = corrected[coating_mask]
    mu_corrected = float(corrected_pixels.mean())
    sigma_corrected = float(corrected_pixels.std())
    uniformity_u = 1.0 - sigma_corrected / mu_corrected if mu_corrected != 0.0 else float("nan")
    return mu_mask_raw, mu_corrected, uniformity_u, corrected


def _compute_coating_zscore_map(corrected: np.ndarray, coating_mask: np.ndarray, mu: float, sigma: float):
    eps = 1e-12
    residual = np.full_like(corrected, np.nan, dtype=np.float64)
    if np.any(coating_mask):
        residual[coating_mask] = (corrected[coating_mask] - mu) / (sigma + eps)
        vmax = float(np.nanpercentile(np.abs(residual[coating_mask]), 99))
        vmax = max(vmax, 1.0)
    else:
        vmax = 1.0
    return residual, vmax


def save_uniformity_heatmap(image_gray: np.ndarray, corrected: np.ndarray, coating_mask: np.ndarray, output_path: str | Path):
    _apply_times_new_roman_style()
    output = Path(output_path)
    output.parent.mkdir(parents=True, exist_ok=True)

    corrected_pixels = corrected[coating_mask]
    mu = float(np.mean(corrected_pixels)) if corrected_pixels.size > 0 else 0.0
    sigma = float(np.std(corrected_pixels)) if corrected_pixels.size > 0 else 1.0

    residual, vmax = _compute_coating_zscore_map(corrected, coating_mask, mu, sigma)
    gray_f = image_gray.astype(np.float64)
    blurred = cv2.GaussianBlur(gray_f, (0, 0), sigmaX=8.0, sigmaY=8.0)
    frosted = np.clip(0.88 * blurred + 26.0, 0.0, 255.0)
    context = gray_f.copy()
    context[~coating_mask] = frosted[~coating_mask]

    fig, ax = plt.subplots(figsize=(7, 6))
    ax.imshow(context, cmap="gray", vmin=0, vmax=255, alpha=0.5)
    heat = np.ma.array(residual, mask=~coating_mask)
    im = ax.imshow(heat, cmap="coolwarm", vmin=-vmax, vmax=vmax, alpha=0.88)
    ax.set_title("Coating Uniformity Heatmap")
    ax.axis("off")
    cbar = plt.colorbar(im, ax=ax, fraction=0.046, pad=0.04)
    cbar.set_label("(I - mu) / sigma")
    fig.tight_layout()
    fig.savefig(output, dpi=300)
    plt.close(fig)


def main() -> None:
    p = argparse.ArgumentParser(description="Single image uniformity heatmap export.")
    p.add_argument("--image", required=True)
    p.add_argument("--mask", required=True)
    p.add_argument("--output", required=True)
    p.add_argument("--gaussian-ksize", type=int, default=101)
    p.add_argument("--gaussian-sigma", type=float, default=0.0)
    p.add_argument("--enable-illumination-correction", action="store_true")
    args = p.parse_args()

    image_gray = read_gray_image(args.image)
    coating_mask = read_coating_mask_non_black(args.mask)
    _, _, _, corrected = evaluate_uniformity_with_mask(
        image_gray=image_gray,
        coating_mask=coating_mask,
        gaussian_ksize=args.gaussian_ksize,
        gaussian_sigma=args.gaussian_sigma,
        apply_illumination_correction=args.enable_illumination_correction,
    )
    save_uniformity_heatmap(image_gray, corrected, coating_mask, args.output)


if __name__ == "__main__":
    main()
