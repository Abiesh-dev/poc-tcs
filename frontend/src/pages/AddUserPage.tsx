import { useState, type ChangeEvent, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from 'primereact/button';
import { ApiError } from '../api/ApiError';
import { createUser } from '../api/users';
import { ErrorMessage } from '../components/ErrorMessage';
import { FormField } from '../components/FormField';
import { useToast } from '../components/toast/ToastContext';
import {
  emptyUserForm,
  mapServerErrors,
  toUserInput,
  validateUserForm,
  type UserFormErrors,
  type UserFormValues,
  MAX_AGE,
  MIN_AGE,
  NAME_MAX_LENGTH,
  NAME_MIN_LENGTH,
  PINCODE_MAX_LENGTH,
  PINCODE_MIN_LENGTH,
} from '../validation/userForm';

export const AddUserPage = () => {
  const navigate = useNavigate();
  const { showToast } = useToast();

  const [values, setValues] = useState<UserFormValues>(emptyUserForm);
  const [errors, setErrors] = useState<UserFormErrors>({});
  const [formError, setFormError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const handleChange = (event: ChangeEvent<HTMLInputElement>) => {
    const { name, value } = event.target;
    setValues((current) => ({ ...current, [name]: value }));
    setErrors((current) => ({ ...current, [name]: undefined }));
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setFormError('');

    const validationErrors = validateUserForm(values);
    setErrors(validationErrors);

    if (Object.keys(validationErrors).length > 0) {
      return;
    }

    setSubmitting(true);

    try {
      await createUser(toUserInput(values));
      showToast('User added successfully.');
      navigate('/');
    } catch (caught) {
      if (caught instanceof ApiError && caught.isValidationError) {
        setErrors(mapServerErrors(caught.fieldErrors));
        setFormError('Please correct the highlighted fields.');
      } else if (caught instanceof ApiError) {
        setFormError(caught.message);
      } else {
        setFormError('Something went wrong while saving the user.');
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <section>
      <div className="page-header">
        <div>
          <h1>Add user</h1>
          <p className="muted">Create a new entry in the directory.</p>
        </div>
      </div>

      <form className="surface-card form" onSubmit={handleSubmit} noValidate>
        {formError && <ErrorMessage message={formError} />}

        <FormField
          id="name"
          name="name"
          label="Name"
          value={values.name}
          onChange={handleChange}
          error={errors.name}
          hint={`${NAME_MIN_LENGTH}–${NAME_MAX_LENGTH} characters`}
          autoComplete="name"
          placeholder="e.g. Aarav Sharma"
        />

        <FormField
          id="age"
          name="age"
          label="Age"
          inputMode="numeric"
          value={values.age}
          onChange={handleChange}
          error={errors.age}
          hint={`${MIN_AGE}–${MAX_AGE}`}
          placeholder="e.g. 32"
        />

        <div className="form__row">
          <FormField
            id="city"
            name="city"
            label="City"
            value={values.city}
            onChange={handleChange}
            error={errors.city}
            autoComplete="address-level2"
            placeholder="e.g. Chennai"
          />

          <FormField
            id="state"
            name="state"
            label="State"
            value={values.state}
            onChange={handleChange}
            error={errors.state}
            autoComplete="address-level1"
            placeholder="e.g. Tamil Nadu"
          />
        </div>

        <FormField
          id="pincode"
          name="pincode"
          label="Pincode"
          value={values.pincode}
          onChange={handleChange}
          error={errors.pincode}
          hint={`${PINCODE_MIN_LENGTH}–${PINCODE_MAX_LENGTH} characters`}
          autoComplete="postal-code"
          placeholder="e.g. 600001"
        />

        <div className="form__actions">
          <Button
            type="button"
            label="Cancel"
            icon="pi pi-times"
            severity="secondary"
            outlined
            onClick={() => navigate('/')}
          />
          <Button
            type="submit"
            label={submitting ? 'Saving…' : 'Save user'}
            icon="pi pi-check"
            loading={submitting}
            disabled={submitting}
          />
        </div>
      </form>
    </section>
  );
};
