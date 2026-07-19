import { useRef, useState, type PointerEvent as ReactPointerEvent, type ReactNode } from "react"

import { Dialog, DialogContent, DialogTitle, DialogTrigger } from "@/shared/ui/dialog"

interface RecipeImageLightboxProps {
  imageUrl: string
  imageAlt: string
  children: ReactNode
}

interface Point {
  x: number
  y: number
}

interface ImageTransform extends Point {
  scale: number
}

interface DragGesture {
  type: "drag"
  startPoint: Point
  startTransform: ImageTransform
  startedOutsideImage: boolean
}

interface PinchGesture {
  type: "pinch"
  startDistance: number
  startScale: number
  anchor: Point
}

type Gesture = DragGesture | PinchGesture

const initialTransform: ImageTransform = { scale: 1, x: 0, y: 0 }
const maxScale = 4
const doubleClickScale = 2.5

export function RecipeImageLightbox({ imageUrl, imageAlt, children }: RecipeImageLightboxProps) {
  const [open, setOpen] = useState(false)

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>{children}</DialogTrigger>
      <DialogContent className="inset-0 flex h-svh max-h-none w-screen max-w-none translate-x-0 translate-y-0 items-center justify-center gap-0 rounded-none border-0 bg-black/90 p-0 text-white shadow-none sm:inset-0 sm:top-0 sm:left-0 sm:h-svh sm:w-screen sm:max-w-none sm:translate-x-0 sm:translate-y-0 sm:rounded-none [&>[data-slot=dialog-close]]:z-20 [&>[data-slot=dialog-close]]:bg-black/55 [&>[data-slot=dialog-close]]:text-white [&>[data-slot=dialog-close]]:opacity-100 [&>[data-slot=dialog-close]]:hover:bg-black/75">
        <DialogTitle className="sr-only">{imageAlt}</DialogTitle>
        <ZoomableImage
          imageUrl={imageUrl}
          imageAlt={imageAlt}
          onRequestClose={() => {
            setOpen(false)
          }}
        />
      </DialogContent>
    </Dialog>
  )
}

function ZoomableImage({
  imageUrl,
  imageAlt,
  onRequestClose,
}: {
  imageUrl: string
  imageAlt: string
  onRequestClose: () => void
}) {
  const containerRef = useRef<HTMLDivElement>(null)
  const imageRef = useRef<HTMLImageElement>(null)
  const pointersRef = useRef(new Map<number, Point>())
  const gestureRef = useRef<Gesture | undefined>(undefined)
  const transformRef = useRef(initialTransform)
  const [transform, setTransform] = useState(initialTransform)
  const [isLoaded, setIsLoaded] = useState(false)

  function updateTransform(nextTransform: ImageTransform) {
    const boundedTransform = constrainTransform(
      nextTransform,
      containerRef.current,
      imageRef.current,
    )
    transformRef.current = boundedTransform
    setTransform(boundedTransform)
  }

  function beginDrag(point: Point, startedOutsideImage = false) {
    gestureRef.current = {
      type: "drag",
      startPoint: point,
      startTransform: transformRef.current,
      startedOutsideImage,
    }
  }

  function beginPinch() {
    const points = [...pointersRef.current.values()]
    const firstPoint = points[0]
    const secondPoint = points[1]

    const midpoint = getMidpoint(firstPoint, secondPoint)
    const currentTransform = transformRef.current
    gestureRef.current = {
      type: "pinch",
      startDistance: Math.max(1, getDistance(firstPoint, secondPoint)),
      startScale: currentTransform.scale,
      anchor: {
        x: (midpoint.x - currentTransform.x) / currentTransform.scale,
        y: (midpoint.y - currentTransform.y) / currentTransform.scale,
      },
    }
  }

  function handlePointerDown(event: ReactPointerEvent<HTMLDivElement>) {
    event.currentTarget.setPointerCapture(event.pointerId)
    const point = getPointerPoint(event, event.currentTarget)
    pointersRef.current.set(event.pointerId, point)

    if (pointersRef.current.size >= 2) {
      beginPinch()
      return
    }

    beginDrag(point, event.target === event.currentTarget)
  }

  function handlePointerMove(event: ReactPointerEvent<HTMLDivElement>) {
    if (!pointersRef.current.has(event.pointerId)) {
      return
    }

    event.preventDefault()
    const point = getPointerPoint(event, event.currentTarget)
    pointersRef.current.set(event.pointerId, point)

    if (pointersRef.current.size >= 2) {
      if (gestureRef.current?.type !== "pinch") {
        beginPinch()
      }

      const gesture = gestureRef.current
      const points = [...pointersRef.current.values()]
      const firstPoint = points[0]
      const secondPoint = points[1]

      if (gesture?.type !== "pinch") {
        return
      }

      const midpoint = getMidpoint(firstPoint, secondPoint)
      const nextScale = clamp(
        gesture.startScale * (getDistance(firstPoint, secondPoint) / gesture.startDistance),
        1,
        maxScale,
      )
      updateTransform({
        scale: nextScale,
        x: midpoint.x - gesture.anchor.x * nextScale,
        y: midpoint.y - gesture.anchor.y * nextScale,
      })
      return
    }

    const gesture = gestureRef.current
    if (gesture?.type !== "drag" || transformRef.current.scale <= 1) {
      return
    }

    updateTransform({
      scale: gesture.startTransform.scale,
      x: gesture.startTransform.x + point.x - gesture.startPoint.x,
      y: gesture.startTransform.y + point.y - gesture.startPoint.y,
    })
  }

  function handlePointerEnd(event: ReactPointerEvent<HTMLDivElement>) {
    const gesture = gestureRef.current
    const endPoint = getPointerPoint(event, event.currentTarget)
    const shouldClose =
      pointersRef.current.size === 1 &&
      gesture?.type === "drag" &&
      gesture.startedOutsideImage &&
      getDistance(gesture.startPoint, endPoint) < 8

    pointersRef.current.delete(event.pointerId)

    if (event.currentTarget.hasPointerCapture(event.pointerId)) {
      event.currentTarget.releasePointerCapture(event.pointerId)
    }

    for (const remainingPoint of pointersRef.current.values()) {
      beginDrag(remainingPoint)
      return
    }

    gestureRef.current = undefined
    if (shouldClose) {
      onRequestClose()
    }
  }

  function handleDoubleClick(event: ReactPointerEvent<HTMLDivElement>) {
    event.preventDefault()

    if (transformRef.current.scale > 1) {
      updateTransform(initialTransform)
      return
    }

    const point = getPointerPoint(event, event.currentTarget)
    updateTransform({
      scale: doubleClickScale,
      x: point.x * (1 - doubleClickScale),
      y: point.y * (1 - doubleClickScale),
    })
  }

  return (
    <div
      ref={containerRef}
      className="relative flex size-full touch-none items-center justify-center overflow-hidden select-none"
      onPointerDown={handlePointerDown}
      onPointerMove={handlePointerMove}
      onPointerUp={handlePointerEnd}
      onPointerCancel={handlePointerEnd}
      onDoubleClick={handleDoubleClick}
    >
      {!isLoaded ? (
        <span
          aria-label="Загружаем изображение"
          className="size-8 animate-spin rounded-full border-2 border-white/30 border-t-white"
        />
      ) : null}
      <img
        ref={imageRef}
        className={`absolute max-h-[calc(100svh-1rem)] max-w-[calc(100vw-1rem)] object-contain transition-opacity duration-200 ${isLoaded ? "opacity-100" : "opacity-0"}`}
        style={{
          transform: `translate3d(${String(transform.x)}px, ${String(transform.y)}px, 0) scale(${String(transform.scale)})`,
        }}
        src={imageUrl}
        alt={imageAlt}
        draggable={false}
        decoding="async"
        fetchPriority="high"
        onLoad={() => {
          setIsLoaded(true)
        }}
      />
    </div>
  )
}

function getPointerPoint(event: ReactPointerEvent, container: HTMLElement): Point {
  const bounds = container.getBoundingClientRect()
  return {
    x: event.clientX - bounds.left - bounds.width / 2,
    y: event.clientY - bounds.top - bounds.height / 2,
  }
}

function getDistance(firstPoint: Point, secondPoint: Point) {
  return Math.hypot(secondPoint.x - firstPoint.x, secondPoint.y - firstPoint.y)
}

function getMidpoint(firstPoint: Point, secondPoint: Point): Point {
  return {
    x: (firstPoint.x + secondPoint.x) / 2,
    y: (firstPoint.y + secondPoint.y) / 2,
  }
}

function constrainTransform(
  transform: ImageTransform,
  container: HTMLDivElement | null,
  image: HTMLImageElement | null,
): ImageTransform {
  if (!container || !image || transform.scale <= 1) {
    return transform.scale <= 1 ? initialTransform : transform
  }

  const maxX = Math.max(0, (image.offsetWidth * transform.scale - container.clientWidth) / 2)
  const maxY = Math.max(0, (image.offsetHeight * transform.scale - container.clientHeight) / 2)

  return {
    scale: transform.scale,
    x: clamp(transform.x, -maxX, maxX),
    y: clamp(transform.y, -maxY, maxY),
  }
}

function clamp(value: number, minimum: number, maximum: number) {
  return Math.min(maximum, Math.max(minimum, value))
}
