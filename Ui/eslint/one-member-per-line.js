// Local ESLint rule: a multiline object, destructuring, interface or type literal has one member per line.
// Inline constructions (`{ x: 1, y: 2 }`) are left alone; @stylistic/object-curly-newline decides when to break them.

/** @type {import('eslint').Rule.RuleModule} */
export default {
  meta: {
    type: 'layout',
    fixable: 'whitespace',
    docs: { description: 'Put each member of a multiline object, interface or type literal on its own line' },
    messages: { chop: 'Put each member of a multiline object on its own line' },
    schema: [],
  },

  create(context) {
    const source = context.sourceCode

    /**
     * Reports every member that starts on the line where the previous member ends
     */
    function check(node, members) {
      if (members.length < 2) {
        return
      }

      // 1. only multiline constructions: the opening and closing brace are on different lines
      const open = source.getFirstToken(node)
      const close = source.getTokenAfter(members.at(-1), (token) => token.value === '}')
      if (!close || open.loc.start.line === close.loc.end.line) {
        return
      }

      // 2. each member after the first has to start on a new line (comments between members count as separators)
      for (let index = 1; index < members.length; index++) {
        const member = members[index]
        const before = source.getTokenBefore(member, { includeComments: true })
        if (before.loc.end.line === member.loc.start.line) {
          context.report({
            node: member,
            messageId: 'chop',
            // the line break replaces the spaces after the separator; @stylistic/indent fixes the indentation
            fix: (fixer) => fixer.replaceTextRange([before.range[1], member.range[0]], '\n'),
          })
        }
      }
    }

    return {
      ObjectExpression: (node) => check(node, node.properties),
      ObjectPattern: (node) => check(node, node.properties),
      TSTypeLiteral: (node) => check(node, node.members),
      TSInterfaceBody: (node) => check(node, node.body),
    }
  },
}
