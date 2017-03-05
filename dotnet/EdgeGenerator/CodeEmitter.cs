/**
 * CodeEmitter class
 * Copyright(c) 2017 Steve Westbrook
 * MIT Licensed
 */

using System;
using System.Text;
using System.Globalization;

namespace EdgeGenerator
{
	public class CodeEmitter
	{
		protected StringBuilder buffer;

		protected const int DefaultIndentWidth = 4;

    protected int currentIndentWidth;

    protected string incrementalIndent;
  
		public CodeEmitter()
      : base()
		{
			buffer = new StringBuilder();
      this.IndentWidth = DefaultIndentWidth;
		}

    public int IndentWidth 
    {
      get 
      {
        return this.incrementalIndent.Length;
      }

      set
      {
				if (value < 0) {
					throw new ArgumentOutOfRangeException();
				}

        this.incrementalIndent = value == 0 ? string.Empty : new string(' ', value);
      }
    }
		
		public StringBuilder Buffer
		{
			get
			{
				return this.buffer;
			}
		}

    protected string CurrentIndent 
    { 
      get 
      {
				return this.currentIndentWidth == 0 ?
					string.Empty :
					new string(' ', this.currentIndentWidth);
      }
    }

    public override string ToString() 
    {
      return this.buffer.ToString();
    }

		public void Indent()
		{
			this.currentIndentWidth += this.IndentWidth;
		}

		public void Outdent()
		{
			this.currentIndentWidth -= this.IndentWidth;

			// Ensure indent is always 0
			if (this.currentIndentWidth < 0) {
				this.currentIndentWidth = 0;
			}
		}

    protected virtual void BlockStart()
    {
      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        "{0}{{",
        this.CurrentIndent);

      this.buffer.AppendLine();
      this.Indent();
    }

    protected void BlockEnd()
    {
      this.Outdent();

      this.buffer.AppendFormat(
        CultureInfo.InvariantCulture,
        "{0}}}",
        this.CurrentIndent);

      this.buffer.AppendLine();
    }

	}
}

