import { Component } from '@angular/core';
import { CardApiService } from './card-api.service';
import { CardLayer, CardTemplateDetails, CardTemplatePayload, CardTemplateSummary } from './card-editor.models';

@Component({
  selector: 'app-card-editor',
  templateUrl: './card-editor.component.html',
  styleUrls: ['./card-editor.component.scss']
})
export class CardEditorComponent {
  readonly previewScale = 0.36;

  cardWidth = 1024;
  cardHeight = 1400;
  layers: CardLayer[] = [];
  selectedLayerId?: string;
  draggingLayerId?: string;
  dragOffsetX = 0;
  dragOffsetY = 0;
  rendering = false;
  currentTemplateId?: string;
  currentTemplateName = 'Template sem nome';
  savedTemplates: CardTemplateSummary[] = [];
  templatesLoading = false;
  templateSaving = false;

  constructor(private readonly cardApi: CardApiService) {
    this.loadClassicTemplate();
    this.loadTemplates();
  }

  get selectedLayer(): CardLayer | undefined {
    return this.layers.find(x => x.id === this.selectedLayerId);
  }

  get orderedLayers(): CardLayer[] {
    return [...this.layers].sort((a, b) => a.zIndex - b.zIndex);
  }

  loadClassicTemplate(): void {
    this.currentTemplateId = undefined;
    this.currentTemplateName = 'Template clássico local';
    this.cardWidth = 1024;
    this.cardHeight = 1400;
    this.layers = [
      this.createTextLayer('overall', '87', 165, 140, 150, 90, 76, 20),
      this.createTextLayer('position', 'ALA', 170, 235, 150, 60, 42, 21),
      this.createImageLayer('base-card', 'Base da carta', undefined, 0, 0, 1024, 1400, 0),
      this.createImageLayer('player', 'Jogador', undefined, 185, 265, 650, 650, 10),
      this.createImageLayer('badge', 'Escudo', undefined, 420, 720, 180, 180, 11),
      this.createTextLayer('name', 'JOGADOR', 512, 950, 760, 90, 72, 30),
      this.createTextLayer('pace', '93 PAC', 235, 1070, 220, 60, 40, 31),
      this.createTextLayer('shot', '88 FIN', 235, 1130, 220, 60, 40, 32),
      this.createTextLayer('pass', '84 PAS', 235, 1190, 220, 60, 40, 33),
      this.createTextLayer('dribbling', '91 DRI', 650, 1070, 220, 60, 40, 34),
      this.createTextLayer('defense', '45 DEF', 650, 1130, 220, 60, 40, 35),
      this.createTextLayer('physical', '78 FIS', 650, 1190, 220, 60, 40, 36)
    ];

    this.selectedLayerId = 'player';
  }

  async onUpload(event: Event, layerId: string): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) return;

    if (file.type !== 'image/png') {
      alert('Envie apenas PNG para preservar transparência.');
      input.value = '';
      return;
    }

    const base64 = await this.fileToBase64(file);
    const layer = this.layers.find(x => x.id === layerId);

    if (layer) {
      layer.imageBase64 = base64;
      this.selectedLayerId = layer.id;
    }

    input.value = '';
  }

  selectLayer(layer: CardLayer): void {
    this.selectedLayerId = layer.id;
  }

  startDrag(event: MouseEvent | TouchEvent, layer: CardLayer): void {
    event.preventDefault();
    this.selectedLayerId = layer.id;
    this.draggingLayerId = layer.id;

    const point = this.getPointer(event);
    this.dragOffsetX = point.x / this.previewScale - layer.x;
    this.dragOffsetY = point.y / this.previewScale - layer.y;
  }

  moveDrag(event: MouseEvent | TouchEvent): void {
    if (!this.draggingLayerId) return;

    const layer = this.layers.find(x => x.id === this.draggingLayerId);
    if (!layer) return;

    const point = this.getPointer(event);
    layer.x = Math.round(point.x / this.previewScale - this.dragOffsetX);
    layer.y = Math.round(point.y / this.previewScale - this.dragOffsetY);
  }

  stopDrag(): void {
    this.draggingLayerId = undefined;
  }

  addTextLayer(): void {
    const layer = this.createTextLayer(
      crypto.randomUUID(),
      'NOVO TEXTO',
      512,
      680,
      600,
      70,
      52,
      this.layers.length + 20
    );

    this.layers.push(layer);
    this.selectedLayerId = layer.id;
  }

  duplicateSelected(): void {
    const selected = this.selectedLayer;
    if (!selected) return;

    const copy: CardLayer = {
      ...structuredClone(selected),
      id: crypto.randomUUID(),
      name: `${selected.name} cópia`,
      x: selected.x + 30,
      y: selected.y + 30,
      zIndex: this.layers.length + 50
    };

    this.layers.push(copy);
    this.selectedLayerId = copy.id;
  }

  removeSelected(): void {
    if (!this.selectedLayerId) return;
    this.layers = this.layers.filter(x => x.id !== this.selectedLayerId);
    this.selectedLayerId = this.layers.length > 0 ? this.layers[this.layers.length - 1].id : undefined;
  }

  moveLayerUp(): void {
    const selected = this.selectedLayer;
    if (!selected) return;
    selected.zIndex += 1;
  }

  moveLayerDown(): void {
    const selected = this.selectedLayer;
    if (!selected) return;
    selected.zIndex -= 1;
  }

  render(): void {
    this.rendering = true;

    this.cardApi.render({
      width: this.cardWidth,
      height: this.cardHeight,
      layers: this.layers
    }).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'carta-final.png';
        a.click();
        URL.revokeObjectURL(url);
        this.rendering = false;
      },
      error: error => {
        console.error(error);
        alert('Erro ao renderizar carta. Confira se o backend está rodando.');
        this.rendering = false;
      }
    });
  }

  saveTemplate(): void {
    const payload = this.buildTemplatePayload();
    this.templateSaving = true;

    const request = this.currentTemplateId
      ? this.cardApi.updateTemplate(this.currentTemplateId, payload)
      : this.cardApi.createTemplate(payload);

    request.subscribe({
      next: template => {
        this.applyTemplate(template);
        this.templateSaving = false;
        this.loadTemplates();
      },
      error: error => {
        console.error(error);
        alert('Erro ao salvar template. Confira a API e a conexão com o PostgreSQL.');
        this.templateSaving = false;
      }
    });
  }

  loadTemplates(): void {
    this.templatesLoading = true;

    this.cardApi.listTemplates().subscribe({
      next: templates => {
        this.savedTemplates = templates;
        this.templatesLoading = false;
      },
      error: error => {
        console.error(error);
        this.templatesLoading = false;
      }
    });
  }

  loadSavedTemplate(templateId: string): void {
    this.cardApi.getTemplate(templateId).subscribe({
      next: template => this.applyTemplate(template),
      error: error => {
        console.error(error);
        alert('Não foi possível carregar o template salvo.');
      }
    });
  }

  duplicateTemplate(template: CardTemplateSummary): void {
    this.cardApi.getTemplate(template.id).subscribe({
      next: details => {
        const copyPayload: CardTemplatePayload = {
          name: `${details.name} cópia`,
          width: details.width,
          height: details.height,
          layers: structuredClone(details.layers)
        };

        this.cardApi.createTemplate(copyPayload).subscribe({
          next: created => {
            this.applyTemplate(created);
            this.loadTemplates();
          },
          error: error => {
            console.error(error);
            alert('Erro ao duplicar template.');
          }
        });
      },
      error: error => {
        console.error(error);
        alert('Erro ao ler o template para duplicar.');
      }
    });
  }

  deleteTemplate(template: CardTemplateSummary): void {
    const confirmed = confirm(`Excluir o template "${template.name}"?`);
    if (!confirmed) return;

    this.cardApi.deleteTemplate(template.id).subscribe({
      next: () => {
        if (this.currentTemplateId === template.id) {
          this.currentTemplateId = undefined;
          this.currentTemplateName = 'Template removido';
        }

        this.loadTemplates();
      },
      error: error => {
        console.error(error);
        alert('Erro ao excluir template.');
      }
    });
  }

  exportTemplate(): void {
    const content = JSON.stringify({
      name: this.currentTemplateName,
      width: this.cardWidth,
      height: this.cardHeight,
      layers: this.layers
    }, null, 2);

    const blob = new Blob([content], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');

    a.href = url;
    a.download = 'template-carta.json';
    a.click();

    URL.revokeObjectURL(url);
  }

  async importTemplate(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) return;

    const text = await file.text();
    const parsed = JSON.parse(text);

    if (!Array.isArray(parsed.layers)) {
      alert('Template inválido.');
      return;
    }

    this.currentTemplateId = undefined;
    this.currentTemplateName = parsed.name || file.name.replace(/\.json$/i, '');
    this.cardWidth = Number(parsed.width) || 1024;
    this.cardHeight = Number(parsed.height) || 1400;
    this.layers = parsed.layers;
    this.selectedLayerId = this.layers[0]?.id;
    input.value = '';
  }

  private createImageLayer(
    id: string,
    name: string,
    imageBase64: string | undefined,
    x: number,
    y: number,
    width: number,
    height: number,
    zIndex: number
  ): CardLayer {
    return {
      id,
      type: 'Image',
      name,
      imageBase64,
      x,
      y,
      width,
      height,
      rotation: 0,
      opacity: 1,
      fontSize: 0,
      colorHex: '#FFFFFF',
      strokeColorHex: '#000000',
      strokeThickness: 0,
      bold: false,
      textAlign: 'Center',
      zIndex
    };
  }

  private createTextLayer(
    id: string,
    text: string,
    x: number,
    y: number,
    width: number,
    height: number,
    fontSize: number,
    zIndex: number
  ): CardLayer {
    return {
      id,
      type: 'Text',
      name: text,
      text,
      x,
      y,
      width,
      height,
      rotation: 0,
      opacity: 1,
      fontName: 'Arial',
      fontSize,
      colorHex: '#FFFFFF',
      strokeColorHex: '#080808',
      strokeThickness: 2,
      bold: true,
      textAlign: 'Center',
      zIndex
    };
  }

  private fileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();

      reader.onload = () => resolve(String(reader.result));
      reader.onerror = reject;

      reader.readAsDataURL(file);
    });
  }

  private getPointer(event: MouseEvent | TouchEvent): { x: number; y: number } {
    const target = event.currentTarget as HTMLElement;
    const rect = target.getBoundingClientRect();
    const source = event instanceof MouseEvent ? event : event.touches[0];

    return {
      x: source.clientX - rect.left,
      y: source.clientY - rect.top
    };
  }

  private applyTemplate(template: CardTemplateDetails): void {
    this.currentTemplateId = template.id;
    this.currentTemplateName = template.name;
    this.cardWidth = template.width;
    this.cardHeight = template.height;
    this.layers = structuredClone(template.layers);
    this.selectedLayerId = this.layers[0]?.id;
  }

  private buildTemplatePayload(): CardTemplatePayload {
    return {
      name: this.currentTemplateName.trim() || 'Template sem nome',
      width: this.cardWidth,
      height: this.cardHeight,
      layers: this.layers
    };
  }
}
