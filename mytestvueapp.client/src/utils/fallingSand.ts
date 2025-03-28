import type { PixelGrid } from "@/entities/PixelGrid";

export default function fallingSand(pixelGrid: PixelGrid) {
  for (let x = 0; x < pixelGrid.width; x++) {
    for (let y = pixelGrid.height - 1; y >= 0; y--) {
      if (pixelGrid.grid[x][y] !== "empty") {
        if (
          y + 1 < pixelGrid.height &&
          pixelGrid.grid[x][y + 1] === "empty"
        ) {
          const below = pixelGrid.grid[x][y + 1];
          pixelGrid.grid[x][y + 1] = pixelGrid.grid[x][y];
          pixelGrid.grid[x][y] = below;
        } else {
          //generate a random number either -1 or 1
          const random = Math.random() > 0.5 ? 1 : -1;

          if (
            y + 1 < pixelGrid.height &&
            x + random < pixelGrid.width &&
            x + random >= 0 &&
            pixelGrid.grid[x + random][y + 1] === "empty"
          ) {
            const belowRight = pixelGrid.grid[x + random][y + 1];
            pixelGrid.grid[x + random][y + 1] = pixelGrid.grid[x][y];
            pixelGrid.grid[x][y] = belowRight;
          }
        }
      }
    }
  }
}
