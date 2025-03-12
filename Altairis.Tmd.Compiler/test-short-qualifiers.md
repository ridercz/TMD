# This is sample correct Tutorial-MD file

This file shows all the features of Tutorial Markdown (TMD) format.

TMD is special format for handbooks and tutorials with numbered steps, written in markdown. The format is designed to be reasonably well rendered by any standard MD rendering engine, but in TMD-aware environment allows for special features, useful for writing labs, tutorials etc.
- - -
The main tutorial building blocks are steps. They are rendered as a table and automatically numbered.
- - -
Steps are separated by `- - -` line markings on separate line (technically, the separators are either `\r\n- - -\r\n` or `\n- - -\n`, so no whitespace is allowed).
- - -
Steps can contain any markdown code including **bold**, _italics_, ~~striketrough~~, `inline code`

    or code blocks.

Of course it can also contain [hyperlinks](https://www.altairis.cz/). Automatic hyperlinks are disabled by default, because many tutorials contain sample or non-working URLs like www.example.com.
- - -
For tutorials, we have to indicate added or removed code.

    Markdown code blocks can't contain formatting
    such as **bold** or ~~striketrough~~.

So we have to use the old-fashioned HTML tags `pre`, `ins` and `del`:

<pre>
Here goes some code
<del>This line is to be deleted.</del>
<ins>This line is to be added.</ins>
</pre>
- - -
Pipe tables are supported:

|Col 1|Col 2|
|-|-|
|Sample|Text|
|Sample|Text|
|Sample|Text|
|Sample|Text|

Grid tables are supported as well:

+--------+-------+
| Col 1  | Col 2 |
+========+=======+
| Sample | Text  |
+--------+-------+
| Sample | Text  |
+--------+-------+
| Sample | Text  |
+--------+-------+
| Footer | Text  |
+--------+-------+


- - -
## Directives
- - -
First line after separator can contain directives in form of HTML comments or special strings, to indicate special handling:
* `<!--$-->` or `($)` denotes non-step text (such as subheadings or introduction or other explanation text). This text is rendered outside the main table. This directive is implied when the text starts with the `#` character (heading).
* `<!--i-->` or `(i)` means information. The step will not be numbered and will have special formatting.
* `<!--!-->` or `(!)` means warning. The step will not be numbered and will have special formatting to catch users attention.
* `<!--dl-->` or `(dl)` means step with download. The step will not be numbered and will have special formatting.
* `<!--#identifier-->` will assign specific name (_identifier_, in this case) to a given step. Other steps can then reference this named step using `[#identifier]`.
    * The identifier must be document-unique.
    * Identifiers are case sensitive.
- - -
## Examples of T/MD directives
- - -
(#mystep)
This is a named step (it's internal name is _mystep_) and will be referenced later.
- - -
(i)
This step is formatted as some _information_ message.
```
Even messages can contain some code etc.
```
- - -
(!)
This step is formatted as some **warning** message.
- - -
(dl)
This step is formatted like download instruction.
- - -
($)
This is just a some text outside the table created using the `($)` directive.
- - -
This step references previous step number [#mystep]. The number is generated automatically.
- - -
You may also create [direct named link](#mystep) to a step.
- - -
This is just a regular step thrown here for a good measure.