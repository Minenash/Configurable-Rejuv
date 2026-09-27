# ConfigurableRejuv

A Deadworks plugin that adds a command to change the amount of rejuvenator credits given by the crystal. \
Defaults to 2/2

### Console Command Syntax

```
dw_rejuv_credits [args]
```

| Args                 | Meaning |
|----------------------|---------|
| *no args*            | Prints out current config |
| `<num>`              | Sets credits to `<num>` for all midbosses |
| `<num1> <num2>`      | Sets credits for first midboss to `<num1>`, and the rest to `<num2>` |
| `<num> <bool>`       | If `<bool>` is `true`, saves to file. File is read on server boot |
| `<num> <num> <bool>` | If `<bool>` is `true`, saves to file. File is read on server boot |

### Config

Stored at `bin/win64/managed/plugin_data/configurable_rejuv.json` and can be manually edited or set via the command above.

Default config:
```json
{
  "First": 2,
  "Rest": 2
}
```