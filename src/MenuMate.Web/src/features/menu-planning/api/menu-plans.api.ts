import { apiClient } from "@/shared/api/client"
import { unwrapApiResponse, unwrapEmptyApiResponse } from "@/shared/api/unwrap"
import type { components } from "@/shared/api/generated/schema"

export type MenuPlan = components["schemas"]["MenuPlanResponse"]
export type MenuPlanItem = components["schemas"]["MenuPlanItemResponse"]
export type CreateMenuPlanRequest = components["schemas"]["CreateMenuPlanRequest"]
export type UpdateMenuPlanRequest = components["schemas"]["UpdateMenuPlanRequest"]
export type CreateMenuPlanItemRequest = components["schemas"]["CreateMenuPlanItemRequest"]
export type UpdateMenuPlanItemRequest = components["schemas"]["UpdateMenuPlanItemRequest"]

export async function getMenuPlans() {
  return unwrapApiResponse<MenuPlan[]>(
    apiClient.GET("/api/menu-plans", {
      params: {},
    }),
  )
}

export async function getMenuPlan(menuPlanId: string) {
  return unwrapApiResponse<MenuPlan>(
    apiClient.GET("/api/menu-plans/{menuPlanId}", {
      params: {
        path: {
          menuPlanId,
        },
      },
    }),
  )
}

export async function createMenuPlan(request: CreateMenuPlanRequest) {
  return unwrapApiResponse<MenuPlan>(
    apiClient.POST("/api/menu-plans", {
      body: request,
    }),
  )
}

export async function updateMenuPlan(menuPlanId: string, request: UpdateMenuPlanRequest) {
  return unwrapApiResponse<MenuPlan>(
    apiClient.PUT("/api/menu-plans/{menuPlanId}", {
      params: {
        path: {
          menuPlanId,
        },
      },
      body: request,
    }),
  )
}

export async function deleteMenuPlan(menuPlanId: string) {
  await unwrapEmptyApiResponse(
    apiClient.DELETE("/api/menu-plans/{menuPlanId}", {
      params: {
        path: {
          menuPlanId,
        },
      },
    }),
  )
}

export async function addMenuPlanItem(menuPlanId: string, request: CreateMenuPlanItemRequest) {
  return unwrapApiResponse<MenuPlan>(
    apiClient.POST("/api/menu-plans/{menuPlanId}/items", {
      params: {
        path: {
          menuPlanId,
        },
      },
      body: request,
    }),
  )
}

export async function removeMenuPlanItem(menuPlanId: string, itemId: string) {
  await unwrapEmptyApiResponse(
    apiClient.DELETE("/api/menu-plans/{menuPlanId}/items/{itemId}", {
      params: {
        path: {
          menuPlanId,
          itemId,
        },
      },
    }),
  )
}

export async function updateMenuPlanItem(
  menuPlanId: string,
  itemId: string,
  request: UpdateMenuPlanItemRequest,
) {
  return unwrapApiResponse<MenuPlan>(
    apiClient.PUT("/api/menu-plans/{menuPlanId}/items/{itemId}", {
      params: {
        path: {
          menuPlanId,
          itemId,
        },
      },
      body: request,
    }),
  )
}
