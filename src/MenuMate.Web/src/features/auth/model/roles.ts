const administratorRole = "admin"

export function isAdministrator(roles: readonly string[]) {
  return roles.some((role) => role.trim().toLowerCase() === administratorRole)
}

export function formatRole(role: string) {
  switch (role.trim().toLowerCase()) {
    case administratorRole:
      return "Администратор"
    case "user":
      return "Пользователь"
    default:
      return role
  }
}
