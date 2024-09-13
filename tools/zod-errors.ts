const leftPad = (stringToPad: string, number: number = 1): string => {
  return '  '.repeat(number) + stringToPad;
};

export type ErrorRoot = {
  _errors: string[];
};
export type ErrorInstance = ErrorRoot & {
  [key: string]: ErrorInstance;
};

export function formatErrors(input: ErrorRoot): any {
  let result: string = '';

  const activeBreadcumb: string[] = [];

  function formatErrorsRecursive(input: any, activeBreadcumb: string[]) {
    if (input['_errors'] && input['_errors'].length > 0) {
      const errorsString = `- ${input['_errors'].join('\n')}\n`;
      result += leftPad(errorsString, activeBreadcumb.length);
    }

    for (const key in input) {
      if (key === '_errors') continue;

      result += `${leftPad(key, activeBreadcumb.length)}:\n`;

      activeBreadcumb.push(key);
      formatErrorsRecursive(input[key], activeBreadcumb);
      activeBreadcumb.pop();
    }
  }

  formatErrorsRecursive(input, activeBreadcumb);

  return result;
}
