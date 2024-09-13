import z from 'zod';

export {};

declare global {
  interface Response {
    /**
     * Return JSON parsed and validated using the passed in schema.
     * If the JSON fails to validate against the schema, a {@link z.ZodError} will be thrown.
     * @type TOutput - The expected returned type
     * @type TSchema - Type of schema to validate against
     * @param schema - A zod schema to validate against
     * @throws z.ZodError
     * @example
     * Passing everything:
     *
     * ```
     * response.parseJson<GetPaymentListResponse, typeof GetPaymentListResponseSchema>(GetPaymentListResponseSchema);
     * // Returns Promise<GetPaymentListResponse>
     * ```
     *
     * @example
     * Inferring everything:
     *
     * ```
     * response.parseJson(GetPaymentListResponseSchema);
     * // Returns Promise<{ ... }> with whatever GetPaymentListResponseSchema defines
     * ```
     */
    parseJson<TSchema extends z.ZodTypeAny>(schema: TSchema): Promise<z.infer<TSchema>>;
    parseJson<TOutput, TSchema extends z.ZodTypeAny>(schema: TSchema): Promise<TOutput>;
    parseJson<TOutput>(schema: z.ZodTypeAny): Promise<TOutput>;
  }
}
