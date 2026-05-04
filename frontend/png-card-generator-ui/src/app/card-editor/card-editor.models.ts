export type CardLayerType = 'Image' | 'Text';

export interface CardLayer {
  id: string;
  type: CardLayerType;
  name: string;

  imageBase64?: string;
  text?: string;

  x: number;
  y: number;
  width: number;
  height: number;

  rotation: number;
  opacity: number;

  fontName?: string;
  fontSize: number;
  colorHex: string;
  strokeColorHex: string;
  strokeThickness: number;
  bold: boolean;
  textAlign: 'Left' | 'Center' | 'Right';

  zIndex: number;
}

export interface CardRenderRequest {
  width: number;
  height: number;
  layers: CardLayer[];
}

export interface CardTemplatePayload extends CardRenderRequest {
  name: string;
}

export interface CardTemplateSummary {
  id: string;
  name: string;
  width: number;
  height: number;
  layerCount: number;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CardTemplateDetails extends CardTemplateSummary {
  layers: CardLayer[];
}
