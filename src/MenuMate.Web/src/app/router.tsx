import { Navigate, createBrowserRouter } from "react-router-dom"

import { AppShell } from "@/app/AppShell"
import { AppShellSkeleton } from "@/app/AppShellSkeleton"
import { AdminRoute } from "@/app/AdminRoute"
import { ProtectedRoute } from "@/app/ProtectedRoute"
import { getLastWorkspaceSection } from "@/app/navigation"
import LoginPage from "@/pages/auth/LoginPage"
import RegisterPage from "@/pages/auth/RegisterPage"
import NotFoundPage from "@/pages/NotFoundPage"
import RecipeDetailsPage from "@/pages/recipes/RecipeDetailsPage"
import RecipesPage from "@/pages/recipes/RecipesPage"
import ShoppingPage from "@/pages/shopping/ShoppingPage"
import { useSessionStore } from "@/shared/auth/session.store"

export const router = createBrowserRouter([
  {
    path: "/login",
    element: <LoginPage />,
  },
  {
    path: "/register",
    element: <RegisterPage />,
  },
  {
    element: <ProtectedRoute />,
    HydrateFallback: AppShellSkeleton,
    children: [
      {
        path: "/",
        element: <AppShell />,
        children: [
          {
            index: true,
            element: <LastWorkspaceSectionRedirect />,
          },
          {
            path: "recipes",
            element: <RecipesPage />,
          },
          {
            path: "recipes/new",
            lazy: async () => ({
              Component: (await import("@/pages/recipes/RecipeCreatePage")).default,
            }),
          },
          {
            path: "recipes/import",
            lazy: async () => ({
              Component: (await import("@/pages/recipes/RecipeImportPage")).default,
            }),
          },
          {
            path: "recipes/import/:draftId",
            lazy: async () => ({
              Component: (await import("@/pages/recipes/RecipeImportDraftPage")).default,
            }),
          },
          {
            path: "recipes/:recipeId",
            element: <RecipeDetailsPage />,
          },
          {
            path: "recipes/:recipeId/copy",
            lazy: async () => ({
              Component: (await import("@/pages/recipes/RecipeCopyPage")).default,
            }),
          },
          {
            path: "recipes/:recipeId/edit",
            lazy: async () => ({
              Component: (await import("@/pages/recipes/RecipeEditPage")).default,
            }),
          },
          {
            path: "menu",
            lazy: async () => ({ Component: (await import("@/pages/menu/MenuPage")).default }),
          },
          {
            path: "shopping",
            element: <ShoppingPage />,
          },
          {
            path: "shopping/preview",
            lazy: async () => ({
              Component: (await import("@/pages/shopping/ShoppingPreviewPage")).default,
            }),
          },
          {
            path: "profile",
            lazy: async () => ({
              Component: (await import("@/pages/profile/ProfilePage")).default,
            }),
          },
          {
            path: "admin",
            element: <AdminRoute />,
            children: [
              {
                index: true,
                lazy: async () => ({
                  Component: (await import("@/pages/admin/AdminUsersPage")).default,
                }),
              },
            ],
          },
        ],
      },
    ],
  },
  {
    path: "*",
    element: <NotFoundPage />,
  },
])

function LastWorkspaceSectionRedirect() {
  const offlineAccess = useSessionStore((state) => state.offlineAccess)
  return <Navigate to={offlineAccess ? "/shopping" : getLastWorkspaceSection()} replace />
}
