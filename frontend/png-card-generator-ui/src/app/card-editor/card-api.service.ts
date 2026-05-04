import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CardRenderRequest } from './card-editor.models';

@Injectable({ providedIn: 'root' })
export class CardApiService {
  private readonly apiUrl = 'https://localhost:7047/api/cards';

  constructor(private readonly http: HttpClient) {}

  render(request: CardRenderRequest) {
    const payload = {
      width: request.width,
      height: request.height,
      layers: request.layers.map(layer => ({
        ...layer,
        type: layer.type === 'Image' ? 1 : 2,
        textAlign:
          layer.textAlign === 'Left'
            ? 1
            : layer.textAlign === 'Right'
              ? 3
              : 2
      }))
    };

    return this.http.post(`${this.apiUrl}/render`, payload, {
      responseType: 'blob'
    });
  }
}
