import { Navigate, createBrowserRouter } from "react-router-dom"

import { AppShell } from "@/app/AppShell"
import { ProtectedRoute } from "@/app/ProtectedRoute"
import LoginPage from "@/pages/auth/LoginPage"
import RegisterPage from "@/pages/auth/RegisterPage"
import MenuPage from "@/pages/menu/MenuPage"
import NotFoundPage from "@/pages/NotFoundPage"
import ProfilePage from "@/pages/profile/ProfilePage"
import RecipeCreatePage from "@/pages/recipes/RecipeCreatePage"
import RecipeDetailsPage from "@/pages/recipes/RecipeDetailsPage"
import RecipeEditPage from "@/pages/recipes/RecipeEditPage"
import RecipeImportDraftPage from "@/pages/recipes/RecipeImportDraftPage"
import RecipeImportPage from "@/pages/recipes/RecipeImportPage"
import RecipesPage from "@/pages/recipes/RecipesPage"
import ShoppingPage from "@/pages/shopping/ShoppingPage"
import ShoppingPreviewPage from "@/pages/shopping/ShoppingPreviewPage"

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
    children: [
      {
        path: "/",
        element: <AppShell />,
        children: [
          {
            index: true,
            element: <Navigate to="/recipes" replace />,
          },
          {
            path: "recipes",
            element: <RecipesPage />,
          },
          {
            path: "recipes/new",
            element: <RecipeCreatePage />,
          },
          {
            path: "recipes/import",
            element: <RecipeImportPage />,
          },
          {
            path: "recipes/import/:draftId",
            element: <RecipeImportDraftPage />,
          },
          {
            path: "recipes/:recipeId",
            element: <RecipeDetailsPage />,
          },
          {
            path: "recipes/:recipeId/edit",
            element: <RecipeEditPage />,
          },
          {
            path: "menu",
            element: <MenuPage />,
          },
          {
            path: "shopping",
            element: <ShoppingPage />,
          },
          {
            path: "shopping/preview",
            element: <ShoppingPreviewPage />,
          },
          {
            path: "profile",
            element: <ProfilePage />,
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
