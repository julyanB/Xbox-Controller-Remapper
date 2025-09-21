export interface VirtualKeyGroup {
  label: string;
  options: { value: string; label: string }[];
}

export const VIRTUAL_KEY_GROUPS: VirtualKeyGroup[] = [
  {
    label: 'WASD & Movement',
    options: [
      { value: 'VK_W', label: 'W' },
      { value: 'VK_A', label: 'A' },
      { value: 'VK_S', label: 'S' },
      { value: 'VK_D', label: 'D' },
      { value: 'VK_Q', label: 'Q' },
      { value: 'VK_E', label: 'E' }
    ]
  },
  {
    label: 'Letters',
    options: Array.from({ length: 26 }, (_, index) => {
      const letter = String.fromCharCode(65 + index);
      return { value: `VK_${letter}`, label: letter };
    })
  },
  {
    label: 'Numbers',
    options: Array.from({ length: 10 }, (_, index) => ({
      value: `VK_${index}`,
      label: `${index}`
    }))
  },
  {
    label: 'Function Keys',
    options: Array.from({ length: 12 }, (_, index) => {
      const f = index + 1;
      return { value: `VK_F${f}`, label: `F${f}` };
    })
  },
  {
    label: 'Arrows & Navigation',
    options: [
      { value: 'VK_UP', label: 'Arrow Up' },
      { value: 'VK_DOWN', label: 'Arrow Down' },
      { value: 'VK_LEFT', label: 'Arrow Left' },
      { value: 'VK_RIGHT', label: 'Arrow Right' },
      { value: 'VK_PRIOR', label: 'Page Up' },
      { value: 'VK_NEXT', label: 'Page Down' },
      { value: 'VK_HOME', label: 'Home' },
      { value: 'VK_END', label: 'End' }
    ]
  },
  {
    label: 'Modifiers',
    options: [
      { value: 'VK_SHIFT', label: 'Shift' },
      { value: 'VK_CONTROL', label: 'Ctrl' },
      { value: 'VK_MENU', label: 'Alt' },
      { value: 'VK_LSHIFT', label: 'Left Shift' },
      { value: 'VK_RSHIFT', label: 'Right Shift' },
      { value: 'VK_LCONTROL', label: 'Left Ctrl' },
      { value: 'VK_RCONTROL', label: 'Right Ctrl' },
      { value: 'VK_LMENU', label: 'Left Alt' },
      { value: 'VK_RMENU', label: 'Right Alt' }
    ]
  },
  {
    label: 'System',
    options: [
      { value: 'VK_SPACE', label: 'Space' },
      { value: 'VK_RETURN', label: 'Enter' },
      { value: 'VK_TAB', label: 'Tab' },
      { value: 'VK_BACK', label: 'Backspace' },
      { value: 'VK_ESCAPE', label: 'Escape' },
      { value: 'VK_CAPITAL', label: 'Caps Lock' },
      { value: 'VK_SNAPSHOT', label: 'Print Screen' },
      { value: 'VK_SCROLL', label: 'Scroll Lock' },
      { value: 'VK_PAUSE', label: 'Pause / Break' }
    ]
  },
  {
    label: 'Numpad',
    options: Array.from({ length: 10 }, (_, index) => ({
      value: `VK_NUMPAD${index}`,
      label: `Numpad ${index}`
    })).concat([
      { value: 'VK_DECIMAL', label: 'Numpad .' },
      { value: 'VK_ADD', label: 'Numpad +' },
      { value: 'VK_SUBTRACT', label: 'Numpad -' },
      { value: 'VK_MULTIPLY', label: 'Numpad *' },
      { value: 'VK_DIVIDE', label: 'Numpad /' }
    ])
  }
];
