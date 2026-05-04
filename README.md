# PNG Card Generator v5

Projeto full-stack para criar cartas estilo FIFA preservando transparência de PNG.

## Recursos da etapa 5

- Backend .NET 8 com ImageSharp
- Renderização por camadas
- Upload de PNG base, jogador/arte destaque e escudo
- Editor visual no Angular
- Drag-and-drop de camadas
- Redimensionamento manual por painel
- Textos editáveis
- Atributos estilo FIFA
- Presets/templates via JSON
- Preview no frontend
- Render final pelo backend em PNG
- Estrutura preparada para salvar templates no banco futuramente

## Estrutura

```txt
backend/
  PngCardGenerator.Api/
frontend/
  png-card-generator-ui/
```

## Rodar backend

```bash
cd backend/PngCardGenerator.Api
dotnet restore
dotnet run
```

API:
```txt
https://localhost:7047/api/cards/render
```

## Rodar frontend

```bash
cd frontend/png-card-generator-ui
npm install
ng serve
```

Frontend:
```txt
http://localhost:4200
```

## Pacotes importantes

Backend:
- SixLabors.ImageSharp
- SixLabors.Fonts
- SixLabors.ImageSharp.Drawing

Frontend:
- Angular
- RxJS

## Observação

A pasta `backend/PngCardGenerator.Api/Assets/Fonts` está preparada para você adicionar fontes reais.
Por padrão, o backend tenta usar fontes do sistema se nenhuma fonte customizada for adicionada.
