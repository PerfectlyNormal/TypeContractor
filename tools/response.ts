import { ZodError, z } from 'zod';
import { formatErrors } from './zod-errors';

Response.prototype.parseJson = async function <TSchema extends z.ZodTypeAny>(schema: TSchema) {
  return await this.parseJson<z.infer<TSchema>, TSchema>(schema);
};

Response.prototype.parseJson = async function <TOutput, TSchema extends z.ZodTypeAny>(schema: TSchema) {
  let input: unknown;
  try {
    input = await this.json();
    const parsed = schema.parse(input);
    return parsed as TOutput;
  } catch (err: unknown) {
    if (err instanceof ZodError) {
        const formattedErrors = formatErrors(err.format());
        console.error(
          `Failed to parse input using schema. Error(s) returned were:\n${formattedErrors}\n\nReceived input:\n`,
          input
        );

      return input as TOutput;
    }

    throw err;
  }
};
