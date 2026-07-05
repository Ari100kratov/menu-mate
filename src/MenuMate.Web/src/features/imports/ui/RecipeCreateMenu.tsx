import { FilePenLine, ImagePlus, Plus } from "lucide-react"
import { useNavigate } from "react-router-dom"

import { Button } from "@/shared/ui/button"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/shared/ui/dropdown-menu"

interface RecipeCreateMenuProps {
  iconOnly?: boolean
  className?: string
}

export function RecipeCreateMenu({ iconOnly = false, className }: RecipeCreateMenuProps) {
  const navigate = useNavigate()

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          size={iconOnly ? "icon-lg" : "default"}
          className={className}
          aria-label={iconOnly ? "Добавить рецепт" : undefined}
        >
          <Plus />
          {iconOnly ? null : "Добавить"}
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem onSelect={() => void navigate("/recipes/new")}>
          <FilePenLine />
          Добавить вручную
        </DropdownMenuItem>
        <DropdownMenuItem onSelect={() => void navigate("/recipes/import")}>
          <ImagePlus />
          Создать по изображению
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  )
}
