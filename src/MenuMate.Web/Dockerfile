FROM node:26-alpine AS build
WORKDIR /app

RUN npm install --global pnpm@11.5.0
COPY src/MenuMate.Web/package.json src/MenuMate.Web/pnpm-lock.yaml src/MenuMate.Web/pnpm-workspace.yaml ./
RUN pnpm install --frozen-lockfile

COPY src/MenuMate.Web ./
RUN pnpm build

FROM nginx:1.31-alpine AS runtime
COPY src/MenuMate.Web/nginx.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
