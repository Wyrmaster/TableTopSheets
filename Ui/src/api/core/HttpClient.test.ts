import { describe, expect, it, vi } from "vitest"
import { DeleteCharacterIntent, GetCharacterIntent, GetCharacterSheetPdfIntent, UpdateCharacterIntent } from "../intents/characters"
import { LoginIntent } from "../intents/auth"
import { ListSpellsIntent } from "../intents/rules"
import { problemMessages } from "./ApiResult"
import { HttpClient, type HttpClientOptions } from "./HttpClient"

function json(status: number, body: unknown, contentType = "application/json") {
  return new Response(JSON.stringify(body), { status, headers: { "Content-Type": contentType } })
}

function client(response: Response | (() => Promise<Response>), options: HttpClientOptions = {}) {
  const fetch = vi.fn<typeof globalThis.fetch>(typeof response === "function" ? response : async () => response)
  return { http: new HttpClient({ fetch, getAccessToken: () => "token-1", ...options }), fetch }
}

function request(fetch: ReturnType<typeof vi.fn<typeof globalThis.fetch>>) {
  const [url, init] = fetch.mock.calls[0]
  return { url: String(url), init: init!, headers: new Headers(init!.headers) }
}

describe("HttpClient", () => {
  it("builds the url with the query and leaves out empty values", async () => {
    const { http, fetch } = client(json(200, { items: [], page: 2, pageSize: 10, totalCount: 0, totalPages: 0 }), { baseUrl: "http://api" })

    const result = await http.execute(new ListSpellsIntent({ class: "wizard", maxLevel: 1, ritual: false, search: undefined, page: 2, pageSize: 10 }))

    expect(result.ok).toBe(true)
    expect(request(fetch).url).toBe("http://api/api/rules/spells?class=wizard&maxLevel=1&ritual=false&page=2&pageSize=10")
    expect(request(fetch).init.method).toBe("GET")
  })

  it("sends the bearer token, but not for anonymous intents", async () => {
    const authenticated = client(json(200, {}))
    await authenticated.http.execute(new GetCharacterIntent("c1"))
    expect(request(authenticated.fetch).headers.get("Authorization")).toBe("Bearer token-1")

    const anonymous = client(json(200, {}))
    await anonymous.http.execute(new ListSpellsIntent())
    expect(request(anonymous.fetch).headers.get("Authorization")).toBeNull()
  })

  it("sends body intents as json", async () => {
    const { http, fetch } = client(json(200, { accessToken: "t" }))

    await http.execute(new UpdateCharacterIntent("c 1", { name: "Elara" }))

    const sent = request(fetch)
    expect(sent.url).toBe("/api/characters/c%201")
    expect(sent.init.method).toBe("PATCH")
    expect(sent.headers.get("Content-Type")).toBe("application/json")
    expect(JSON.parse(sent.init.body as string)).toEqual({ name: "Elara" })
  })

  it("returns the parsed json", async () => {
    const { http } = client(json(200, { accessToken: "t", tokenType: "Bearer" }))

    const result = await http.execute(new LoginIntent({ userName: "a", password: "b" }))

    expect(result).toEqual({ ok: true, status: 200, data: { accessToken: "t", tokenType: "Bearer" } })
  })

  it("reads 204 and responseKind none as undefined", async () => {
    const { http } = client(new Response(null, { status: 204 }))

    const result = await http.execute(new DeleteCharacterIntent("c1"))

    expect(result).toEqual({ ok: true, status: 204, data: undefined })
  })

  it("reads blobs", async () => {
    const { http } = client(new Response(new Blob(["%PDF"], { type: "application/pdf" }), { status: 200, headers: { "Content-Type": "application/pdf" } }))

    const result = await http.execute(new GetCharacterSheetPdfIntent("c1"))

    expect(result.ok && result.data instanceof Blob).toBe(true)
    expect(result.ok && (await result.data.text())).toBe("%PDF")
  })

  it("returns problem details of rule violations", async () => {
    const { http } = client(json(422, { title: "The request breaks the rules", status: 422, errors: { rules: ["'actor' is not an available option"] } }, "application/problem+json"))

    const result = await http.execute(new GetCharacterIntent("c1"))

    expect(result.ok).toBe(false)
    if (!result.ok) {
      expect(result.status).toBe(422)
      expect(problemMessages(result.problem)).toEqual(["'actor' is not an available option"])
    }
  })

  it("falls back to the status text without problem details", async () => {
    const { http } = client(new Response("oops", { status: 500, statusText: "Internal Server Error" }))

    const result = await http.execute(new GetCharacterIntent("c1"))

    expect(result).toEqual({ ok: false, status: 500, problem: { status: 500, title: "Internal Server Error" } })
  })

  it("calls onUnauthorized on 401 of authenticated intents only", async () => {
    const onUnauthorized = vi.fn()
    await client(new Response(null, { status: 401 }), { onUnauthorized }).http.execute(new GetCharacterIntent("c1"))
    expect(onUnauthorized).toHaveBeenCalledTimes(1)

    await client(new Response(null, { status: 401 }), { onUnauthorized }).http.execute(new LoginIntent({ userName: "a", password: "b" }))
    expect(onUnauthorized).toHaveBeenCalledTimes(1)
  })

  it("reports network errors instead of throwing", async () => {
    const { http } = client(() => Promise.reject(new TypeError("Failed to fetch")))

    const result = await http.execute(new GetCharacterIntent("c1"))

    expect(result).toEqual({ ok: false, status: 0, problem: { title: "Network error", detail: "Failed to fetch" } })
  })

  it("reports aborted requests", async () => {
    const controller = new AbortController()
    const { http } = client(() => {
      controller.abort()
      return Promise.reject(new DOMException("aborted", "AbortError"))
    })

    const result = await http.execute(new GetCharacterIntent("c1"), controller.signal)

    expect(result.ok === false && result.aborted).toBe(true)
  })
})
