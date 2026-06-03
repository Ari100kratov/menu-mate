import { Toaster as Sonner, type ToasterProps } from "sonner"

function Toaster(props: ToasterProps) {
  return <Sonner richColors closeButton toastOptions={{ className: "font-sans" }} {...props} />
}

export { Toaster }
