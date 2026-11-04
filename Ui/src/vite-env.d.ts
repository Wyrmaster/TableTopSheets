/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** major.minor.build of the build; set by the API Dockerfile from the pipeline's version, missing in `pnpm dev` */
  readonly VITE_APP_VERSION?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
