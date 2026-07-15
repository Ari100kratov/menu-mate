const maxImageEdge = 1920
const reencodeThresholdBytes = 750 * 1024
const webpQuality = 0.82

export async function optimizeRecipeImageForUpload(file: File) {
  if (typeof createImageBitmap !== "function") {
    return file
  }

  let bitmap: ImageBitmap

  try {
    bitmap = await createImageBitmap(file)
  } catch {
    return file
  }

  try {
    const scale = Math.min(1, maxImageEdge / Math.max(bitmap.width, bitmap.height))

    if (scale === 1 && file.size <= reencodeThresholdBytes) {
      return file
    }

    const width = Math.max(1, Math.round(bitmap.width * scale))
    const height = Math.max(1, Math.round(bitmap.height * scale))
    const canvas = document.createElement("canvas")
    canvas.width = width
    canvas.height = height

    const context = canvas.getContext("2d")
    if (!context) {
      return file
    }

    context.drawImage(bitmap, 0, 0, width, height)
    const optimizedBlob = await canvasToBlob(canvas)

    if (!optimizedBlob || optimizedBlob.size >= file.size) {
      return file
    }

    return new File([optimizedBlob], replaceFileExtension(file.name, ".webp"), {
      type: optimizedBlob.type,
      lastModified: file.lastModified,
    })
  } finally {
    bitmap.close()
  }
}

function canvasToBlob(canvas: HTMLCanvasElement) {
  return new Promise<Blob | null>((resolve) => {
    canvas.toBlob(resolve, "image/webp", webpQuality)
  })
}

function replaceFileExtension(fileName: string, extension: string) {
  const extensionStart = fileName.lastIndexOf(".")
  const baseName = extensionStart > 0 ? fileName.slice(0, extensionStart) : fileName
  return `${baseName}${extension}`
}
