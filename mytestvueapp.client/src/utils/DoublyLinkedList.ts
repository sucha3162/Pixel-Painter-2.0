class NodeT<T> {
  value: T;
  next: NodeT<T> | null;
  prev: NodeT<T> | null;

  constructor(value: T) {
    this.value = value;
    this.next = null;
    this.prev = null;
  }
}

export class DoublyLinkedList<T> {
  head: NodeT<T> | null;
  tail: NodeT<T> | null;
  current: NodeT<T> | null;
  length: number;

  constructor() {
    this.head = null;
    this.tail = null;
    this.current = null;
    this.length = 0;
  }



  //DLL DATA MANIPULATION

  // Add to the end
  append(value: T): void {
    const newNode = new NodeT(value);
    if (!this.head) {
      this.head = this.tail = newNode;
    } else {
      newNode.prev = this.tail;
      if (this.tail) this.tail.next = newNode;
      this.tail = newNode;
    }
    this.length++;
  }

  // Add to the beginning
  prepend(value: T): void {
    const newNode = new NodeT(value);
    if (!this.head) {
      this.head = this.tail = newNode;
    } else {
      newNode.next = this.head;
      this.head.prev = newNode;
      this.head = newNode;
    }
    this.length++;
  }

  // Remove from end
  pop(): T | null {
    if (!this.tail) return null;
    const value = this.tail.value;
    if (this.tail.prev) {
      this.tail = this.tail.prev;
      this.tail.next = null;
    } else {
      this.head = this.tail = null;
    }
    this.length--;
    return value;
  }

  // Remove from beginning
  shift(): T | null {
    if (!this.head) return null;
    const value = this.head.value;
    if (this.head.next) {
      this.head = this.head.next;
      this.head.prev = null;
    } else {
      this.head = this.tail = null;
    }
    this.length--;
    return value;
  }

  // Convert to array for debugging
  toArray(): T[] {
    const result: T[] = [];
    let current = this.head;
    while (current) {
      result.push(current.value);
      current = current.next;
    }
    return result;
  }




  //DLL GETTERS

  size(): number {
    return this.length;
  }

  getPrev(): T | null {
    return this.current?.prev ? this.current.prev.value : null;
  }

  getNext(): T | null {
    return this.current?.next ? this.current.next.value : null;
  }



  //DLL ITERATION TOOLS

  prev(): T | null {
    if (this.current && this.current.prev) {
      this.current = this.current.prev;
      return this.current.value;
    }
    return null; // Already at the beginning
  }

  next(): T | null {
    if (this.current && this.current.next) {
      this.current = this.current.next;
      return this.current.value;
    }
    return null; // Already at the end
  }

  resetCursor(): void {
    this.current = this.head;
  }

  begin(): T | null {
    if (this.head) {
      this.current = this.head;
      return this.head.value;
    }
    return null; // Empty list
  }

  end(): T | null {
    if (this.tail) {
      this.current = this.tail;
      return this.tail.value;
    }
    return null; // Empty list
  }
}