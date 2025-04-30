export default class DefaultColor {
  hex: string;
  shortcut: string;

  constructor(hex: string, shortcut: string) {
    this.hex = hex;
    this.shortcut = shortcut;
  }
  static getDefaultColors(): DefaultColor[] {
    return [
      new DefaultColor("000000", "Shortcut: 1"),
      new DefaultColor("ED1C24", "Shortcut: 2"),
      new DefaultColor("FF7F27", "Shortcut: 3"),
      new DefaultColor("7F7F7F", "Shortcut: 4"),
      new DefaultColor("FFF200", "Shortcut: 5"),
      new DefaultColor("22B14C", "Shortcut: 6"),
      new DefaultColor("C3C3C3", "Shortcut: 7"),
      new DefaultColor("00A2E8", "Shortcut: 8"),
      new DefaultColor("3F48CC", "Shortcut: 9"),
      new DefaultColor("FFFFFF", "Shortcut: 0"),
      new DefaultColor("A349A4", "Shortcut: -"),
      new DefaultColor("FFAEC9", "Shortcut: =")
    ];
  }
}
  