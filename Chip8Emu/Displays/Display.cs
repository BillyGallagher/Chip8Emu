using Chip8Emu.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Chip8Emu.Displays
{
    public abstract class Display
    {
        protected Vector2 _position, _size;
        protected List<Control> _controls;

        public Display(Vector2 position, Vector2 size) 
        {
            _position = position;
            _size = size;
            _controls = new List<Control>();
        }

        public void AddControl(Control control)
        {
            _controls.Add(control);
        }

        public virtual void Update(GameTime gameTime, KeyboardState keyboardState, MouseState mouseState)
        {
            _controls.ForEach(x => x.Update(gameTime, keyboardState, mouseState));
        }

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _controls.ForEach(x => x.Draw(spriteBatch, gameTime));
        }

        public virtual void LoadContent(GraphicsDevice graphicsDevice)
        {
            _controls.ForEach(x => x.LoadContent(graphicsDevice));
        }
    }
}
