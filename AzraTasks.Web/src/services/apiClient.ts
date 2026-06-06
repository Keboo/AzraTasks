class ApiClient {
  private baseUrl = __API_BASE_URL__ || ''

  async get<T>(url: string): Promise<T> {
    const response = await fetch(this.baseUrl + url, {
      credentials: 'include',
    })
    
    return this.readResponse<T>(response)
  }

  async post<T = void>(url: string, data?: unknown): Promise<T> {
    const response = await fetch(this.baseUrl + url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: data ? JSON.stringify(data) : undefined,
    })
    
    return this.readResponse<T>(response)
  }

  async delete<T = void>(url: string): Promise<T> {
    const response = await fetch(this.baseUrl + url, {
      method: 'DELETE',
      credentials: 'include',
    })
    
    return this.readResponse<T>(response)
  }

  async put<T = void>(url: string, data?: unknown): Promise<T> {
    const response = await fetch(this.baseUrl + url, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: data ? JSON.stringify(data) : undefined,
    })
    
    return this.readResponse<T>(response)
  }

  private async readResponse<T>(response: Response): Promise<T> {
    const text = await response.text()
    let body: { message?: string; error?: string; title?: string } | undefined

    if (text) {
      try {
        body = JSON.parse(text) as { message?: string; error?: string; title?: string }
      } catch {
        body = undefined
      }
    }

    if (!response.ok) {
      const message =
        body?.message ||
        body?.error ||
        body?.title ||
        text ||
        `HTTP error! status: ${response.status}`

      throw new Error(message)
    }

    return (body ?? undefined) as T
  }
}

export const apiClient = new ApiClient()
