FROM node:13.12.0-alpine as build
WORKDIR /app
COPY . .
RUN npm install
RUN npm run build

FROM nginx:1.17.8-alpine
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]