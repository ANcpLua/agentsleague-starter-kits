FROM node:22.12-alpine@sha256:51eff88af6dff26f59316b6e356188ffa2c422bd3c3b76f2556a2e7e89d080bd AS builder

WORKDIR /app

# Copy package.json and package-lock.json first for better layer caching
COPY package*.json ./
COPY tsconfig.json ./
COPY src/ ./src/

RUN --mount=type=cache,target=/root/.npm npm install

RUN npm run build

FROM node:22-alpine@sha256:ab07539e0988b63558ff621f5fbe1077054c39d9809112974fb79993949d41cd AS release

WORKDIR /app

COPY --from=builder /app/dist ./dist
COPY --from=builder /app/package.json ./
COPY --from=builder /app/package-lock.json ./

ENV NODE_ENV=production
ENV PORT=80

RUN npm ci --ignore-scripts --omit-dev

CMD ["node", "dist/server.js"]