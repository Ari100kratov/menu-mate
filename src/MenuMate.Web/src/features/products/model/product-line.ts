export interface ProductLineEditorValue {
  productId: string
  productName: string
  amount: string
  unit: string
  isToTaste: boolean
  category: string
  comment: string
}

export type ProductLineEditorErrors = Partial<
  Record<Extract<keyof ProductLineEditorValue, string>, string>
>
