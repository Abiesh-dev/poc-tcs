import { describe, expect, it } from 'vitest';
import {
  emptyUserForm,
  mapServerErrors,
  toUserInput,
  validateUserForm,
  type UserFormValues,
} from './userForm';

const validForm: UserFormValues = {
  name: 'Ada Lovelace',
  age: '36',
  city: 'London',
  state: 'Greater London',
  pincode: '600001',
};

describe('validateUserForm', () => {
  it('accepts a fully populated form', () => {
    expect(validateUserForm(validForm)).toEqual({});
  });

  it('reports every missing field on an empty form', () => {
    const errors = validateUserForm(emptyUserForm);

    expect(Object.keys(errors).sort()).toEqual(['age', 'city', 'name', 'pincode', 'state']);
  });

  it('rejects a whitespace-only name', () => {
    expect(validateUserForm({ ...validForm, name: '   ' }).name).toBe('Name is required.');
  });

  it('rejects a name shorter than two characters', () => {
    expect(validateUserForm({ ...validForm, name: 'A' }).name).toBe(
      'Name must be between 2 and 100 characters.',
    );
  });

  it('rejects a name longer than one hundred characters', () => {
    expect(validateUserForm({ ...validForm, name: 'a'.repeat(101) }).name).toBeDefined();
  });

  it.each(['abc', '3.5', '-1', '1e3'])('rejects a non-integer age of %s', (age) => {
    expect(validateUserForm({ ...validForm, age }).age).toBeDefined();
  });

  it('rejects an age above the maximum', () => {
    expect(validateUserForm({ ...validForm, age: '121' }).age).toBe('Age must be between 0 and 120.');
  });

  it.each(['0', '120'])('accepts the boundary age %s', (age) => {
    expect(validateUserForm({ ...validForm, age }).age).toBeUndefined();
  });

  it('requires city and state', () => {
    const errors = validateUserForm({ ...validForm, city: ' ', state: '' });

    expect(errors.city).toBe('City is required.');
    expect(errors.state).toBe('State is required.');
  });

  it.each(['123', '12345678901'])('rejects pincode %s outside the length range', (pincode) => {
    expect(validateUserForm({ ...validForm, pincode }).pincode).toBe(
      'Pincode must be between 4 and 10 characters.',
    );
  });

  it.each(['1234', '1234567890'])('accepts the boundary pincode %s', (pincode) => {
    expect(validateUserForm({ ...validForm, pincode }).pincode).toBeUndefined();
  });

  it('ignores surrounding whitespace when measuring length', () => {
    expect(validateUserForm({ ...validForm, pincode: '  600001  ' }).pincode).toBeUndefined();
  });
});

describe('toUserInput', () => {
  it('trims text and converts the age to a number', () => {
    const input = toUserInput({
      name: '  Ada Lovelace ',
      age: ' 36 ',
      city: ' London ',
      state: ' Greater London ',
      pincode: ' 600001 ',
    });

    expect(input).toEqual({
      name: 'Ada Lovelace',
      age: 36,
      city: 'London',
      state: 'Greater London',
      pincode: '600001',
    });
  });
});

describe('mapServerErrors', () => {
  it('maps API field names onto form fields regardless of casing', () => {
    const errors = mapServerErrors({
      Name: ['Name is required.'],
      Pincode: ['Pincode must be between 4 and 10 characters.'],
    });

    expect(errors).toEqual({
      name: 'Name is required.',
      pincode: 'Pincode must be between 4 and 10 characters.',
    });
  });

  it('ignores fields that do not exist on the form', () => {
    expect(mapServerErrors({ somethingElse: ['Nope.'] })).toEqual({});
  });

  it('keeps only the first message per field', () => {
    expect(mapServerErrors({ name: ['First.', 'Second.'] })).toEqual({ name: 'First.' });
  });
});
