namespace PopForums {

    export interface ImageLimits {
        maxWidth: number;
        maxHeight: number;
        maxkBytes: number;
        // fill the whole box and trim the overflow (avatars), instead of fitting inside it
        isCrop: boolean;
        quality: number;
    }

    // Images are resized in the browser before upload, and the server only checks the header of what it
    // receives, so it never has to decode untrusted image data. Keep the admin copy in Admin.js in sync.
    export class ImageResizer {
        // Already within limits, these upload untouched: GIF keeps its animation, PNG stays sharp.
        private static passThroughTypes = ["image/png", "image/gif"];
        private static minQuality = 0.4;

        static async prepare(file: File, limits: ImageLimits): Promise<File> {
            const maxBytes = limits.maxkBytes * 1024;
            const image = await ImageResizer.load(file);
            const width = image.naturalWidth;
            const height = image.naturalHeight;
            if (ImageResizer.passThroughTypes.includes(file.type) && file.size <= maxBytes && width <= limits.maxWidth && height <= limits.maxHeight)
                return file;

            // JPEGs always get re-encoded, even when small enough, so EXIF data (like GPS location) is
            // stripped and the rotation is baked in. The server reads unrotated dimensions from the header.
            const target = ImageResizer.getTarget(width, height, limits);
            const canvas = ImageResizer.draw(image, target);
            // only sharpen what was actually scaled down, not a JPEG that was just re-encoded or trimmed
            if (target.dw < target.sw || target.dh < target.sh)
                ImageResizer.sharpen(canvas);
            for (let quality = limits.quality; quality >= ImageResizer.minQuality - 0.01; quality -= 0.1) {
                const blob = await ImageResizer.toBlob(canvas, quality);
                if (blob.size <= maxBytes) {
                    const name = file.name.replace(/\.[^.]*$/, "") + ".jpg";
                    return new File([blob], name, { type: "image/jpeg" });
                }
            }
            throw new Error(ImageResizer.errorMessage());
        }

        private static load(file: File): Promise<HTMLImageElement> {
            // An img element (unlike createImageBitmap in older Safari) applies EXIF orientation everywhere.
            const url = URL.createObjectURL(file);
            const image = new Image();
            image.src = url;
            return image.decode()
                .then(() => image)
                .catch(() => { throw new Error(ImageResizer.errorMessage()); })
                .finally(() => URL.revokeObjectURL(url));
        }

        private static getTarget(width: number, height: number, limits: ImageLimits) {
            if (!limits.isCrop) {
                const scale = Math.min(1, limits.maxWidth / width, limits.maxHeight / height);
                return { sx: 0, sy: 0, sw: width, sh: height, dw: Math.max(1, Math.round(width * scale)), dh: Math.max(1, Math.round(height * scale)) };
            }
            // never scale up; an image narrower than the box keeps its width and only gets trimmed in height, and vice versa
            const scale = Math.min(1, Math.max(limits.maxWidth / width, limits.maxHeight / height));
            const dw = Math.max(1, Math.min(limits.maxWidth, Math.round(width * scale)));
            const dh = Math.max(1, Math.min(limits.maxHeight, Math.round(height * scale)));
            const sw = Math.min(width, dw / scale);
            const sh = Math.min(height, dh / scale);
            return { sx: (width - sw) / 2, sy: (height - sh) / 2, sw: sw, sh: sh, dw: dw, dh: dh };
        }

        private static draw(image: HTMLImageElement, target: { sx: number, sy: number, sw: number, sh: number, dw: number, dh: number }): HTMLCanvasElement {
            // Halving in steps looks far better than one big jump down, and the first step being half size
            // keeps huge phone photos under iOS Safari's canvas memory limit.
            let source: CanvasImageSource = image;
            let { sx, sy, sw, sh } = target;
            while (sw / 2 >= target.dw && sh / 2 >= target.dh) {
                const step = ImageResizer.createCanvas(Math.round(sw / 2), Math.round(sh / 2));
                step.getContext("2d")!.drawImage(source, sx, sy, sw, sh, 0, 0, step.width, step.height);
                source = step;
                sx = 0;
                sy = 0;
                sw = step.width;
                sh = step.height;
            }
            const canvas = ImageResizer.createCanvas(target.dw, target.dh);
            const context = canvas.getContext("2d")!;
            // JPEG has no transparency, and without this transparent areas come out black
            context.fillStyle = "#fff";
            context.fillRect(0, 0, canvas.width, canvas.height);
            context.drawImage(source, sx, sy, sw, sh, 0, 0, target.dw, target.dh);
            return canvas;
        }

        // Matches the light GaussianSharpen(0.5) the server used to apply after resizing: a separable
        // unsharp kernel (twice the original minus a Gaussian blur), run along rows and then down columns.
        private static sharpen(canvas: HTMLCanvasElement): void {
            const context = canvas.getContext("2d")!;
            const { width, height } = canvas;
            const image = context.getImageData(0, 0, width, height);
            const data = image.data;
            const sigma = 0.5;
            // Gaussian weight one pixel from center, relative to center; the two-pixel taps are under 0.1% and dropped
            const side = Math.exp(-1 / (2 * sigma * sigma));
            const edge = -side / (1 + 2 * side);
            const center = 1 - 2 * edge;
            const rows = new Float32Array(data.length);
            for (let y = 0; y < height; y++) {
                for (let x = 0; x < width; x++) {
                    const i = (y * width + x) * 4;
                    const left = (y * width + Math.max(x - 1, 0)) * 4;
                    const right = (y * width + Math.min(x + 1, width - 1)) * 4;
                    for (let c = 0; c < 3; c++)
                        rows[i + c] = center * data[i + c] + edge * (data[left + c] + data[right + c]);
                }
            }
            for (let y = 0; y < height; y++) {
                for (let x = 0; x < width; x++) {
                    const i = (y * width + x) * 4;
                    const up = (Math.max(y - 1, 0) * width + x) * 4;
                    const down = (Math.min(y + 1, height - 1) * width + x) * 4;
                    // alpha is left alone; it's always opaque after the white fill
                    for (let c = 0; c < 3; c++)
                        data[i + c] = center * rows[i + c] + edge * (rows[up + c] + rows[down + c]);
                }
            }
            context.putImageData(image, 0, 0);
        }

        private static createCanvas(width: number, height: number): HTMLCanvasElement {
            const canvas = document.createElement("canvas");
            canvas.width = width;
            canvas.height = height;
            const context = canvas.getContext("2d")!;
            context.imageSmoothingEnabled = true;
            context.imageSmoothingQuality = "high";
            return canvas;
        }

        private static toBlob(canvas: HTMLCanvasElement, quality: number): Promise<Blob> {
            return new Promise((resolve, reject) => {
                canvas.toBlob(blob => {
                    // null means the browser couldn't allocate the canvas, usually because the image is enormous
                    if (blob)
                        resolve(blob);
                    else
                        reject(new Error(ImageResizer.errorMessage()));
                }, "image/jpeg", quality);
            });
        }

        private static errorMessage(): string {
            return PopForums.localizations?.imageProcessFailed ?? "This image couldn't be processed.";
        }
    }
}
