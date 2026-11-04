/** Longest edge of an uploaded portrait in pixels; larger pictures are scaled down before the upload */
const MaxEdge = 512

/** JPEG quality of the scaled picture */
const Quality = 0.85

const loadImage = (file: File) => new Promise<HTMLImageElement>((resolve, reject) => {
  const url = URL.createObjectURL(file)
  const image = new Image()
  image.onload = () => {
    URL.revokeObjectURL(url)
    resolve(image)
  }
  image.onerror = () => {
    URL.revokeObjectURL(url)
    reject(new Error('The file is no image'))
  }
  image.src = url
})

const toBase64 = (blob: Blob) => new Promise<string>((resolve, reject) => {
  const reader = new FileReader()
  // the result is a data url (`data:image/jpeg;base64,…`); the API wants the part after the comma
  reader.onload = () => resolve(String(reader.result).split(',')[1] ?? '')
  reader.onerror = () => reject(reader.error ?? new Error('The picture could not be read'))
  reader.readAsDataURL(blob)
})

/**
 * A picture file as base64 JPEG for `SetPortraitIntent`, scaled down to at most 512 px on its longest edge, so phone
 * photos of several megabytes become a few dozen kilobytes
 */
export const portraitData = async (file: File) => {
  const image = await loadImage(file)
  const scale = Math.min(1, MaxEdge / Math.max(image.naturalWidth, image.naturalHeight))
  const canvas = document.createElement('canvas')
  canvas.width = Math.round(image.naturalWidth * scale)
  canvas.height = Math.round(image.naturalHeight * scale)
  canvas.getContext('2d')?.drawImage(image, 0, 0, canvas.width, canvas.height)
  const blob = await new Promise<Blob | null>((resolve) => canvas.toBlob(resolve, 'image/jpeg', Quality))
  if (!blob) {
    throw new Error('The picture could not be converted')
  }

  return toBase64(blob)
}
