import Cropper, { type Area, type Point } from "react-easy-crop"
import { Crop, ImageOff, LoaderCircle, ZoomIn } from "lucide-react"
import { useEffect, useMemo, useRef, useState } from "react"
import { toast } from "sonner"

import { Button } from "@/shared/ui/button"
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from "@/shared/ui/dialog"
import { ErrorAlert } from "@/shared/ui/feedback"
import { Skeleton } from "@/shared/ui/skeleton"
import { Slider } from "@/shared/ui/slider"
import { RecipeImage } from "./RecipeImage"

const coordinateScale = 1000
const maximumOutputSize = 1024
const lowResolutionThreshold = 320

export interface RecipeCoverSuggestion {
  id: string
  previewUrl?: string
  loadSourceImage: (signal?: AbortSignal) => Promise<Blob>
  x: number
  y: number
  width: number
  height: number
}

interface RecipeCoverSuggestionCardProps {
  suggestion: RecipeCoverSuggestion
  onApply: (file: File) => void
  onDismiss: () => void
}

interface LoadedSourceImage {
  objectUrl: string
  width: number
  height: number
}

export function RecipeCoverSuggestionCard({
  suggestion,
  onApply,
  onDismiss,
}: RecipeCoverSuggestionCardProps) {
  const [isOpen, setIsOpen] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [isApplying, setIsApplying] = useState(false)
  const [error, setError] = useState<unknown>()
  const [sourceImage, setSourceImage] = useState<LoadedSourceImage>()
  const [crop, setCrop] = useState<Point>({ x: 0, y: 0 })
  const [zoom, setZoom] = useState(1)
  const [croppedAreaPixels, setCroppedAreaPixels] = useState<Area>()
  const requestControllerRef = useRef<AbortController>(null)

  const initialCroppedAreaPercentages = useMemo(
    () =>
      sourceImage
        ? createInitialSquareCrop(suggestion, sourceImage.width, sourceImage.height)
        : undefined,
    [sourceImage, suggestion],
  )

  useEffect(
    () => () => {
      requestControllerRef.current?.abort()
    },
    [],
  )

  useEffect(
    () => () => {
      if (sourceImage) {
        URL.revokeObjectURL(sourceImage.objectUrl)
      }
    },
    [sourceImage],
  )

  async function openEditor() {
    requestControllerRef.current?.abort()
    const controller = new AbortController()
    requestControllerRef.current = controller
    setError(undefined)
    setSourceImage(undefined)
    setCroppedAreaPixels(undefined)
    setCrop({ x: 0, y: 0 })
    setZoom(1)
    setIsOpen(true)
    setIsLoading(true)

    try {
      const blob = await suggestion.loadSourceImage(controller.signal)
      const objectUrl = URL.createObjectURL(blob)

      try {
        const dimensions = await getImageDimensions(objectUrl)
        if (controller.signal.aborted) {
          URL.revokeObjectURL(objectUrl)
          return
        }

        setSourceImage({ objectUrl, ...dimensions })
      } catch (loadError) {
        URL.revokeObjectURL(objectUrl)
        throw loadError
      }
    } catch (loadError) {
      if (!controller.signal.aborted) {
        setError(loadError)
      }
    } finally {
      if (!controller.signal.aborted) {
        setIsLoading(false)
      }
    }
  }

  function handleOpenChange(open: boolean) {
    setIsOpen(open)
    if (!open) {
      requestControllerRef.current?.abort()
      requestControllerRef.current = null
      setSourceImage(undefined)
      setError(undefined)
      setIsLoading(false)
      setIsApplying(false)
    }
  }

  async function applyCrop() {
    if (!sourceImage || !croppedAreaPixels) {
      return
    }

    setError(undefined)
    setIsApplying(true)
    try {
      const file = await createCroppedCoverFile(sourceImage.objectUrl, croppedAreaPixels)
      onApply(file)
      handleOpenChange(false)
      toast.success("Фото из источника добавлено как обложка")
    } catch (cropError) {
      setError(cropError)
    } finally {
      setIsApplying(false)
    }
  }

  const objectPosition = `${String((suggestion.x + suggestion.width / 2) / 10)}% ${String((suggestion.y + suggestion.height / 2) / 10)}%`
  const isLowResolution =
    croppedAreaPixels !== undefined &&
    Math.min(croppedAreaPixels.width, croppedAreaPixels.height) < lowResolutionThreshold

  return (
    <>
      <div className="bg-muted/30 mt-3 rounded-lg border p-3">
        <div className="flex items-center gap-3">
          {suggestion.previewUrl ? (
            <RecipeImage
              frameClassName="size-16 shrink-0 rounded-lg"
              imageClassName="object-cover"
              src={suggestion.previewUrl}
              alt="Предложенное фото готового блюда"
              style={{ objectPosition }}
            />
          ) : (
            <div className="bg-muted text-muted-foreground flex size-16 shrink-0 items-center justify-center rounded-lg">
              <Crop className="size-6" />
            </div>
          )}
          <div className="min-w-0">
            <p className="type-subsection-title">Найдено фото готового блюда</p>
            <p className="type-supporting text-muted-foreground mt-1">
              Проверьте рамку перед использованием обложки.
            </p>
          </div>
        </div>
        <div className="mt-3 flex flex-col gap-2 sm:flex-row">
          <Button
            type="button"
            variant="outline"
            className="sm:flex-1"
            onClick={() => void openEditor()}
          >
            <Crop />
            Настроить и использовать
          </Button>
          <Button type="button" variant="ghost" onClick={onDismiss}>
            <ImageOff />
            Не использовать
          </Button>
        </div>
      </div>

      <Dialog open={isOpen} onOpenChange={handleOpenChange}>
        <DialogContent className="grid-rows-[auto_minmax(0,1fr)_auto] sm:max-w-xl">
          <DialogHeader>
            <DialogTitle>Настройка обложки</DialogTitle>
          </DialogHeader>

          <div className="min-h-0 overflow-y-auto px-5 pb-5">
            {isLoading ? <Skeleton className="aspect-square w-full rounded-xl" /> : null}

            {error ? <ErrorAlert error={error} /> : null}

            {sourceImage ? (
              <>
                <div className="bg-muted relative aspect-square w-full overflow-hidden rounded-xl">
                  <Cropper
                    key={sourceImage.objectUrl}
                    image={sourceImage.objectUrl}
                    crop={crop}
                    zoom={zoom}
                    minZoom={1}
                    maxZoom={8}
                    aspect={1}
                    objectFit="contain"
                    initialCroppedAreaPercentages={initialCroppedAreaPercentages}
                    onCropChange={setCrop}
                    onZoomChange={setZoom}
                    onCropComplete={(_croppedArea, areaPixels) => {
                      setCroppedAreaPixels(areaPixels)
                    }}
                  />
                </div>

                <div className="mt-4 flex items-center gap-3">
                  <ZoomIn className="text-muted-foreground size-4 shrink-0" />
                  <Slider
                    aria-label="Масштаб изображения"
                    min={1}
                    max={8}
                    step={0.05}
                    value={[zoom]}
                    onValueChange={([nextZoom]) => {
                      setZoom(nextZoom)
                    }}
                  />
                </div>

                {isLowResolution ? (
                  <p className="type-supporting text-muted-foreground mt-3">
                    Выбранный фрагмент небольшой и может выглядеть размытым на крупных экранах.
                  </p>
                ) : null}
              </>
            ) : null}
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => {
                handleOpenChange(false)
              }}
            >
              Отмена
            </Button>
            <Button
              type="button"
              disabled={!sourceImage || !croppedAreaPixels || isApplying}
              onClick={() => void applyCrop()}
            >
              {isApplying ? <LoaderCircle className="animate-spin" /> : <Crop />}
              {isApplying ? "Подготавливаем..." : "Использовать как обложку"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  )
}

function createInitialSquareCrop(
  suggestion: RecipeCoverSuggestion,
  imageWidth: number,
  imageHeight: number,
): Area {
  const x = (suggestion.x / coordinateScale) * imageWidth
  const y = (suggestion.y / coordinateScale) * imageHeight
  const width = (suggestion.width / coordinateScale) * imageWidth
  const height = (suggestion.height / coordinateScale) * imageHeight
  const side = Math.max(1, Math.min(width, height))
  const squareX = x + (width - side) / 2
  const squareY = y + (height - side) / 2

  return {
    x: (squareX / imageWidth) * 100,
    y: (squareY / imageHeight) * 100,
    width: (side / imageWidth) * 100,
    height: (side / imageHeight) * 100,
  }
}

function getImageDimensions(source: string): Promise<{ width: number; height: number }> {
  return new Promise((resolve, reject) => {
    const image = new Image()
    image.onload = () => {
      resolve({ width: image.naturalWidth, height: image.naturalHeight })
    }
    image.onerror = () => {
      reject(new Error("Не удалось открыть исходное изображение."))
    }
    image.src = source
  })
}

async function createCroppedCoverFile(source: string, area: Area): Promise<File> {
  const image = await loadImage(source)
  const outputSize = Math.max(
    1,
    Math.min(maximumOutputSize, Math.floor(Math.min(area.width, area.height))),
  )
  const canvas = document.createElement("canvas")
  canvas.width = outputSize
  canvas.height = outputSize
  const context = canvas.getContext("2d")
  if (!context) {
    throw new Error("Браузер не поддерживает подготовку изображения.")
  }

  context.fillStyle = "#ffffff"
  context.fillRect(0, 0, outputSize, outputSize)
  context.drawImage(image, area.x, area.y, area.width, area.height, 0, 0, outputSize, outputSize)

  const blob = await canvasToBlob(canvas)
  return new File([blob], "recipe-cover-from-source.jpg", { type: "image/jpeg" })
}

function loadImage(source: string): Promise<HTMLImageElement> {
  return new Promise((resolve, reject) => {
    const image = new Image()
    image.onload = () => {
      resolve(image)
    }
    image.onerror = () => {
      reject(new Error("Не удалось обработать исходное изображение."))
    }
    image.src = source
  })
}

function canvasToBlob(canvas: HTMLCanvasElement): Promise<Blob> {
  return new Promise((resolve, reject) => {
    canvas.toBlob(
      (blob) => {
        if (blob) {
          resolve(blob)
          return
        }

        reject(new Error("Не удалось подготовить файл обложки."))
      },
      "image/jpeg",
      0.88,
    )
  })
}
