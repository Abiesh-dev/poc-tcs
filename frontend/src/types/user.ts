export interface User {
  id: number;
  name: string;
  age: number;
  city: string;
  state: string;
  pincode: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface UserInput {
  name: string;
  age: number;
  city: string;
  state: string;
  pincode: string;
}
