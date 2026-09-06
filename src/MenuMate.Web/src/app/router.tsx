import { Navigate, createBrowserRouter } from "react-router-dom"

import { AppShell } from "@/app/AppShell"
import { AdminRoute } from "@/app/AdminRoute"
import { ProtectedRoute } from "@/app/ProtectedRoute"
import { PrivacyPolicyGate } from "@/app/PrivacyPolicyGate"
import { getLastWorkspaceSection } from "@/app/navigation"
import LoginPage from "@/pages/auth/LoginPage"
import RegisterPage from "@/pages/auth/RegisterPage"
import ForgotPasswordPage from "@/pages/auth/ForgotPasswordPage"
import ResetPasswordPage from "@/pages/auth/ResetPasswordPage"
import VerifyEmailPage from "@/pages/auth/VerifyEmailPage"
import AdminUsersPage from "@/pages/admin/AdminUsersPage"
import MenuPage from "@/pages/menu/MenuPage"
import NotFoundPage from "@/pages/NotFoundPage"
import AccountDeletionPage from "@/pages/legal/AccountDeletionPage"
import PrivacyConsentPage from "@/pages/legal/PrivacyConsentPage"
import PrivacyPolicyPage from "@/pages/legal/PrivacyPolicyPage"
import ProfilePage from "@/pages/profile/ProfilePage"
import ConfirmEmailChangePage from "@/pages/profile/ConfirmEmailChangePage"
import RecipeCreatePage from "@/pages/recipes/RecipeCreatePage"
import RecipeCopyPage from "@/pages/recipes/RecipeCopyPage"
import RecipeDetailsPage from "@/pages/recipes/RecipeDetailsPage"
import RecipeEditPage from "@/pages/recipes/RecipeEditPage"
import RecipeImportDraftPage from "@/pages/recipes/RecipeImportDraftPage"
import RecipeImportPage from "@/pages/recipes/RecipeImportPage"
import RecipesPage from "@/pages/recipes/RecipesPage"
import ShoppingPage from "@/pages/shopping/ShoppingPage"
import ShoppingPreviewPage from "@/pages/shopping/ShoppingPreviewPage"
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
    path: "/verify-email",
    element: <VerifyEmailPage />,
  },
  {
    path: "/forgot-password",
    element: <ForgotPasswordPage />,
  },
  {
    path: "/reset-password",
    element: <ResetPasswordPage />,
  },
  {
    path: "/privacy",
    element: <PrivacyPolicyPage />,
  },
  {
    path: "/account-deletion",
    element: <AccountDeletionPage />,
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: "/privacy/accept",
        element: <PrivacyConsentPage />,
      },
      {
        element: <PrivacyPolicyGate />,
        children: [
          {
            path: "/",
            element: <AppShell />,
            children: [
              {
                index: true,
                element: <LastWorkspaceSectionRedirect />,
              },
              { path: "recipes", element: <RecipesPage /> },
              { path: "recipes/new", element: <RecipeCreatePage /> },
              { path: "recipes/import", element: <RecipeImportPage /> },
              { path: "recipes/import/:draftId", element: <RecipeImportDraftPage /> },
              { path: "recipes/:recipeId", element: <RecipeDetailsPage /> },
              { path: "recipes/:recipeId/copy", element: <RecipeCopyPage /> },
              { path: "recipes/:recipeId/edit", element: <RecipeEditPage /> },
              { path: "menu", element: <MenuPage /> },
              { path: "shopping", element: <ShoppingPage /> },
              { path: "shopping/preview", element: <ShoppingPreviewPage /> },
              { path: "profile", element: <ProfilePage /> },
              {
                path: "profile/email-change/confirm",
                element: <ConfirmEmailChangePage />,
              },
              {
                path: "admin",
                element: <AdminRoute />,
                children: [{ index: true, element: <AdminUsersPage /> }],
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
