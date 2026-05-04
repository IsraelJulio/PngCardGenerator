import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import {
  CardRenderRequest,
  CardTemplateDetails,
  CardTemplatePayload,
  CardTemplateSummary,
} from "./card-editor.models";

@Injectable({ providedIn: "root" })
export class CardApiService {
  private readonly cardsApiUrl = "https://localhost:53024/api/cards";
  private readonly templatesApiUrl = "https://localhost:53024/api/templates";

  constructor(private readonly http: HttpClient) {}

  render(request: CardRenderRequest) {
    return this.http.post(`${this.cardsApiUrl}/render`, request, {
      responseType: "blob",
    });
  }

  renderFromTemplate(templateId: string) {
    return this.http.post(
      `${this.cardsApiUrl}/render-from-template/${templateId}`,
      null,
      {
        responseType: "blob",
      },
    );
  }

  listTemplates() {
    return this.http.get<CardTemplateSummary[]>(this.templatesApiUrl);
  }

  getTemplate(id: string) {
    return this.http.get<CardTemplateDetails>(`${this.templatesApiUrl}/${id}`);
  }

  createTemplate(payload: CardTemplatePayload) {
    return this.http.post<CardTemplateDetails>(this.templatesApiUrl, payload);
  }

  updateTemplate(id: string, payload: CardTemplatePayload) {
    return this.http.put<CardTemplateDetails>(
      `${this.templatesApiUrl}/${id}`,
      payload,
    );
  }

  deleteTemplate(id: string) {
    return this.http.delete(`${this.templatesApiUrl}/${id}`);
  }
}
