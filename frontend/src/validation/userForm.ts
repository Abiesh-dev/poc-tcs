import type { UserInput } from '../types/user';

export interface UserFormValues {
  name: string;
  age: string;
  city: string;
  state: string;
  pincode: string;
}

export type UserFormErrors = Partial<Record<keyof UserFormValues, string>>;

export const emptyUserForm: UserFormValues = {
  name: '',
  age: '',
  city: '',
  state: '',
  pincode: '',
};

export const NAME_MIN_LENGTH = 2;
export const NAME_MAX_LENGTH = 100;
export const MIN_AGE = 0;
export const MAX_AGE = 120;
export const PINCODE_MIN_LENGTH = 4;
export const PINCODE_MAX_LENGTH = 10;

export const validateUserForm = (values: UserFormValues): UserFormErrors => {
  const errors: UserFormErrors = {};

  const name = values.name.trim();
  if (!name) {
    errors.name = 'Name is required.';
  } else if (name.length < NAME_MIN_LENGTH || name.length > NAME_MAX_LENGTH) {
    errors.name = `Name must be between ${NAME_MIN_LENGTH} and ${NAME_MAX_LENGTH} characters.`;
  }

  const age = values.age.trim();
  if (!age) {
    errors.age = 'Age is required.';
  } else if (!/^\d+$/.test(age)) {
    errors.age = 'Age must be a whole number.';
  } else if (Number(age) < MIN_AGE || Number(age) > MAX_AGE) {
    errors.age = `Age must be between ${MIN_AGE} and ${MAX_AGE}.`;
  }

  if (!values.city.trim()) {
    errors.city = 'City is required.';
  }

  if (!values.state.trim()) {
    errors.state = 'State is required.';
  }

  const pincode = values.pincode.trim();
  if (!pincode) {
    errors.pincode = 'Pincode is required.';
  } else if (pincode.length < PINCODE_MIN_LENGTH || pincode.length > PINCODE_MAX_LENGTH) {
    errors.pincode = `Pincode must be between ${PINCODE_MIN_LENGTH} and ${PINCODE_MAX_LENGTH} characters.`;
  }

  return errors;
};

export const toUserInput = (values: UserFormValues): UserInput => ({
  name: values.name.trim(),
  age: Number(values.age.trim()),
  city: values.city.trim(),
  state: values.state.trim(),
  pincode: values.pincode.trim(),
});

const FIELD_NAMES: Record<string, keyof UserFormValues> = {
  name: 'name',
  age: 'age',
  city: 'city',
  state: 'state',
  pincode: 'pincode',
};

export const mapServerErrors = (fieldErrors: Record<string, string[]>): UserFormErrors => {
  const errors: UserFormErrors = {};

  for (const [key, messages] of Object.entries(fieldErrors)) {
    const field = FIELD_NAMES[key.toLowerCase()];
    if (field && messages.length > 0) {
      errors[field] = messages[0];
    }
  }

  return errors;
};
