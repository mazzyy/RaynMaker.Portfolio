FROM node:13.12.0-alpine
# set working directory
WORKDIR /app
COPY . .
RUN npm install --silent

EXPOSE 8080
# start app
CMD ["npm", "run", "serve"]