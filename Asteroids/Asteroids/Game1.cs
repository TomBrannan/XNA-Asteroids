using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Asteroids
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        const bool MOUSE_VISIBLE = false;
        const bool FULLSCREEN = false;

        const int WIDTH = 950;
        const int HEIGHT = 700;
        const int MISSILE_REFRESH = 500; //milliseconds between missiles
        const int NUM_ASTEROIDS = 15;

        const int SHIP_COLLISION_OFFSET = 10;
        const float SHIP_SCALE = 0.15f;
        const float SHIP_SPEED = 0.5f;

        const int MISSILE_COLLISION_OFFSET = 5;
        const float MISSILE_SCALE = 0.10f;
        const float MISSILE_SPEED = 2.0f;

        const int ASTEROID_COLLISION_OFFSET = 5;

        Vector2 PROJECTILE_OFFSET;
        Vector2 SHIP_OFFSET;
        Vector2 SHIP_POSITION;
        Vector2 ASTEROID_OFFSET;

        readonly Color FONT_COLOR = Color.Yellow;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        Texture2D earth;
        Texture2D stars;
        Texture2D explosion;
        Texture2D missile;
        Texture2D particle;
        Texture2D shuttle;
        Texture2D asteroid;

        SoundEffect explode;
        SoundEffect gunshot;
        SoundEffect impact;

        Explosion explosionEffect;

        UserControlledSprite ship;
        List<ProjectileSprite> projectiles;
        List<Texture2D> particles;
        List<BouncingSprite> asteroids;
        Random rand;
        int missileFireTime = -MISSILE_REFRESH; //Time in milliseconds when the last missile was fired
        float missileScreenPadding; //How far off the screen the missiles go before being removed
        bool lost = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.IsFullScreen = FULLSCREEN;
            this.IsMouseVisible = MOUSE_VISIBLE;
            graphics.PreferredBackBufferHeight = HEIGHT;
            graphics.PreferredBackBufferWidth = WIDTH;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            rand = new Random(System.DateTime.Now.Millisecond);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            projectiles = new List<ProjectileSprite>();
            particles = new List<Texture2D>();
            asteroids = new List<BouncingSprite>();
            spriteFont = Content.Load<SpriteFont>("font");

            //Load graphics
            earth = Content.Load<Texture2D>("am_rend_all");
            stars = Content.Load<Texture2D>("stars");
            explosion = Content.Load<Texture2D>("explosion");
            missile = Content.Load<Texture2D>("missile");
            particle = Content.Load<Texture2D>("particle");
            shuttle = Content.Load<Texture2D>("shuttle1");
            asteroid = Content.Load<Texture2D>("asteroid");

            //Load sound effects
            explode = Content.Load<SoundEffect>("Explode");
            gunshot = Content.Load<SoundEffect>("Gunshot");
            impact = Content.Load<SoundEffect>("impact");

            explosionEffect = new Explosion(explosion);
            particles.Add(particle);

            missileScreenPadding = missile.Height * MISSILE_SCALE * 2;

            SHIP_POSITION = new Vector2(WIDTH / 2, HEIGHT / 2);
            SHIP_OFFSET = new Vector2(shuttle.Width / 2, shuttle.Height / 2);
            PROJECTILE_OFFSET = new Vector2(missile.Width / 2, 0);
            ASTEROID_OFFSET = new Vector2(asteroid.Width / 2, asteroid.Height / 2);

            ship = new UserControlledSprite(shuttle, SHIP_POSITION, SHIP_COLLISION_OFFSET, SHIP_SPEED, SHIP_OFFSET, SHIP_SCALE);
            
            //Initialize asteroids with random attributes
            for (int i = 0; i < NUM_ASTEROIDS; i++)
            {
                int paddingFromCenter = 85;
                bool top = rand.Next(2) == 1;
                bool left = rand.Next(2) == 1;
                int x = 0;
                int y = 0;

                y = top ? rand.Next(0, HEIGHT / 2 - paddingFromCenter) : rand.Next(HEIGHT / 2 + paddingFromCenter, HEIGHT);
                x = left ? rand.Next(0, WIDTH / 2 - paddingFromCenter) : rand.Next(WIDTH / 2 + paddingFromCenter, WIDTH);

                Vector2 pos = new Vector2((float)x, (float)y);
                Vector2 dir = new Vector2((float)rand.NextDouble()*2 - 1, (float)rand.NextDouble()*2 - 1);
                dir.Normalize();
                float speed = (float)rand.NextDouble();
                float scale = rand.Next(75, 115) / 100.0f;
                float spinRate = rand.Next(15, 50) / 1000.0f;
                asteroids.Add(new BouncingSprite(asteroid, pos, ASTEROID_COLLISION_OFFSET,
                    speed, ASTEROID_OFFSET, scale, spinRate, dir, GraphicsDevice.Viewport.Bounds));
            }
        }

        protected override void Update(GameTime gameTime)
        {
            //Escape - quits the game
            if (Keyboard.GetState().GetPressedKeys().Contains<Keys>(Keys.Escape))
            {
                this.Exit();
            }

            explosionEffect.Update(gameTime);
            if (!lost)
            {
                //Space - fires a missile
                if (Keyboard.GetState().GetPressedKeys().Contains<Keys>(Keys.Space))
                {
                    fireMissile((int)gameTime.TotalGameTime.TotalMilliseconds);
                }

                ship.Update(gameTime, GraphicsDevice.Viewport.Bounds);

                List<int> asteroidsToRemove = new List<int>();
                List<int> missilesToRemove = new List<int>();

                //Update missiles and remove ones that have gone off screen
                float x, y;
                for (int i = 0; i < projectiles.Count; i++)
                {
                    projectiles[i].Update(gameTime, GraphicsDevice.Viewport.Bounds);
                    x = projectiles[i].Position.X;
                    y = projectiles[i].Position.Y;
                    if (x < -missileScreenPadding || x > WIDTH + missileScreenPadding ||
                       y < -missileScreenPadding || y > HEIGHT + missileScreenPadding)
                    {
                        missilesToRemove.Add(i);
                    }
                }



                //Update asteroids and handle collision with ship and missiles
                bool done = false;
                for (int i = 0; i < asteroids.Count && !done; i++)
                {
                    asteroids[i].Update(gameTime, GraphicsDevice.Viewport.Bounds);
                    if (ship.CollisionRect.Intersects(asteroids[i].CollisionRect))
                    {
                        lost = true;
                        explode.Play();
                        explosionEffect.AddExplosion(ship.Position, 75, 1000, 3500, gameTime);
                        asteroidsToRemove.Add(i);
                    }
                    for (int j = 0; j < projectiles.Count && !done; j++)
                    {
                        projectiles[j].Update(gameTime, GraphicsDevice.Viewport.Bounds);
                        if (asteroids[i].CollisionRect.Intersects(projectiles[j].CollisionRect))
                        {
                            done = true;
                            gunshot.Play();
                            asteroidsToRemove.Add(i);
                            missilesToRemove.Add(j);
                            explosionEffect.AddExplosion(asteroids[i].Position, 20, 100, 1000, gameTime);
                        }
                    }
                }

                //Remove asteroids and missiles that have collided
                for (int i = asteroidsToRemove.Count - 1; i >= 0; i--)
                {
                    asteroids.RemoveAt(asteroidsToRemove[i]);
                }
                for (int i = missilesToRemove.Count - 1; i >= 0; i--)
                {
                    projectiles.RemoveAt(missilesToRemove[i]);
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            
            Rectangle bounds = GraphicsDevice.Viewport.Bounds;
            Vector2 earthPosition = new Vector2(WIDTH - (0.85f)*earth.Width, HEIGHT - (1.05f)*earth.Height);

            //Draw background
            spriteBatch.Draw(stars, bounds, Color.White);
            spriteBatch.Draw(earth, earthPosition, earth.Bounds, Color.White);

            //Draw missiles, ship, and asteroids
            foreach (ProjectileSprite ps in projectiles)
            {
                ps.Draw(gameTime, spriteBatch);
            }
            foreach (BouncingSprite bs in asteroids)
            {
                bs.Draw(gameTime, spriteBatch);
            }
            if (!lost)
            {
                ship.Draw(gameTime, spriteBatch);
            }

            //Display a message if the user has won or lost.
            if (lost)
            {
                spriteBatch.DrawString(spriteFont, "You Lose.", new Vector2(50, 50), FONT_COLOR);
            }
            if (asteroids.Count == 0)
            {
                spriteBatch.DrawString(spriteFont, "You Win!", new Vector2(50, 50), FONT_COLOR);
            }

            explosionEffect.Draw(spriteBatch);
            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void fireMissile(int fireTime)
        {
            if (fireTime - missileFireTime > MISSILE_REFRESH)
            {
                impact.Play();
                missileFireTime = fireTime;

                //Fire the missiles near the front of the ship instead of from the center
                float dist = shuttle.Width / 2 * SHIP_SCALE;
                Vector2 tip = new Vector2((float)(ship.Position.X + dist * Math.Cos(ship.Rotation - Math.PI / 2)),
                    (float)(ship.Position.Y + dist * Math.Sin(ship.Rotation - Math.PI / 2)));
                projectiles.Add(new ProjectileSprite(missile, tip, MISSILE_COLLISION_OFFSET,
                   MISSILE_SPEED, PROJECTILE_OFFSET, MISSILE_SCALE, ship.Rotation, particles));
            }
        }
    }
}
