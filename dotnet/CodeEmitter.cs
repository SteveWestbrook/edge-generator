using System;
using System.Text;

namespace EdgeReference
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
	}
}

