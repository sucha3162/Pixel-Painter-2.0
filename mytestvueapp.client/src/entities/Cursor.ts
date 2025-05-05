import type PainterTool from "./PainterTool";
import type { Vector2 } from "./Vector2";

export default class Cursor {
  position: Vector2;
  selectedTool: PainterTool;
  size: number;
  color: string;

  constructor(
    position: Vector2,
    selectedTool: PainterTool,
    size: number,
    color: string
  ) {
    this.position = position;
    this.selectedTool = selectedTool;
    this.size = size;
    this.color = color;
  }
  updateCursor(size: number): void {
    this.size=size;
  }
}
