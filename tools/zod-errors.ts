const leftPad = (stringToPad: string, number: number = 1): string => {
  return '  '.repeat(number) + stringToPad;
};

export type ErrorRoot = {
  errors: string[];
};
export type ErrorInstance = ErrorRoot & {
  [key: string]: ErrorInstance;
};

export function formatErrors(input: ErrorRoot): any {
  let result: string = '';

  const activeBreadcumb: string[] = [];

  function formatErrorsRecursive(input: any, activeBreadcumb: string[]) {
    if (input['errors'] && input['errors'].length > 0) {
      const errorsString = `- ${input['errors'].join('\n')}\n`;
      result += leftPad(errorsString, activeBreadcumb.length);
    }

    for (const key in input) {
      if (key === 'errors') continue;

      result += `${leftPad(key, activeBreadcumb.length)}:\n`;

      activeBreadcumb.push(key);
      formatErrorsRecursive(input[key], activeBreadcumb);
      activeBreadcumb.pop();
    }
  }

  formatErrorsRecursive(input, activeBreadcumb);

  return result;
}
