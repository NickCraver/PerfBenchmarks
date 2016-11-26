using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [Config(typeof(Config))]
    public class TrimTests
    {
        private readonly char[] _whitespaceChars;
        private const string TestPost = @"<p>I’m Kien. I write  English is not very well ,i have a issue ..
    I have a game project .It’s same brickout game project  on the internet.I want to display two words,  each word on each line.They are joined by the bricks block. Inside ,top line is first name align left,bottom line is lastname align right.They are inputed from textbox(as image)
    <a href=""http://i.stack.imgur.com/Hl5Ga.png"" rel=""nofollow"">enter image description here</a>
    Beside each second the screen display option the number of brick (as a variable, Example 5 bricks per second)until the two words appear complete. I displayed a letter of the alphabet which is created from the matrix(0,1).But I don’t know how to join them into one word . 
    May you help me. you can edit this project below,make same demo, or give me a suggestion about algorithm  or keywords on the internet. Thanks a lot
    Below the project by corona sdk.</p>

    <p><a href=""http://www.mediafire.com/download/1t89ftkbznkn184/Breakout2.rar"" rel=""nofollow"">http://www.mediafire.com/download/1t89ftkbznkn184/Breakout2.rar</a></p>

    <p>if you not want to download ,you can view 2 files below</p>

    <p>Bricks.lua</p>

    <pre><code>local Bricks = display.newGroup() -- static object
    local Events = require(""Events"")
    local Levels = require(""Levels"")
    local sound = require(""Sound"")
    local physics = require(""physics"")
    local Sprites = require(""Sprites"")
    local Func = require(""Func"")


    local brickSpriteData = 
    {
        {
            name = ""brick"",
            frames = {Sprites.brick}
        },

        {
            name = ""brick2"",
            frames = {Sprites.brick2}
        },

        {
            name = ""brick3"",
            frames = {Sprites.brick3}
        },

    }

    -- animation table
    local brickAnimations = {}

    Sprites:CreateAnimationTable
    {
        spriteData = brickSpriteData,
        animationTable = brickAnimations
    }

    -- get size from temp object for later use
    local tempBrick = display.newImage('red_apple_20.png',300,500)
    --local tempBrick = display.newImage('cheryGreen2.png',300,500)
    local brickSize =
    {
        width = tempBrick.width, 
        height = tempBrick.height
    }
    --tempBrick:removeSelf( )


    ----------------
    -- Rubble -- needs to be moved to its own file
    ----------------

    local rubbleSpriteData =
    {
        {
            name = ""rubble1"",
            frames = {Sprites.rubble1}
        },

        {
            name = ""rubble2"",
            frames = {Sprites.rubble2}
        },

        {
            name = ""rubble3"",
            frames = {Sprites.rubble3}
        },

        {
            name = ""rubble4"",
            frames = {Sprites.rubble4}
        },

        {
            name = ""rubble5"",
            frames = {Sprites.rubble5}
        },

    }

    local rubbleAnimations = {}
    Sprites:CreateAnimationTable
    {
        spriteData = rubbleSpriteData,
        animationTable = rubbleAnimations
    }

    local totalBricksBroken = 0 -- used to track when level is complete
    local totalBricksAtStart = 0

    -- contains all brick objects
    local bricks = {}


    local function CreateBrick(data)

        -- random brick sprite
        local obj = display.newImage('red_apple_20.png')
        local objGreen = display.newImage('cheryGreen2.png')
        obj.name = ""brick""
        obj.x = data.x --or display.contentCenterX
        obj.y = data.y --or 1000
        obj.brickType = data.brickType or 1
        obj.index = data.index

        function obj:Break()

            totalBricksBroken =  totalBricksBroken + 1
            bricks[self.index] = nil
            obj:removeSelf( )
            sound.play(sound.breakBrick)

        end

        function obj:Update()
            if(self == nil) then
                return
            end 

            if(self.y &gt; display.contentHeight - 20) then
                obj:Break()
            end 
        end 
        if(obj.brickType ==1) then
            physics.addBody( obj, ""static"", {friction=0.5, bounce=0.5 } )
        elseif(obj.brickType == 2) then
            physics.addBody( objGreen,""static"",{friction=0.2, bounce=0.5, density = 1 } )
        end 

        return obj
    end

    local currentLevel = testLevel
    -- create level from bricks defined in an object
    -- this allows for levels to be designed
    local function CreateBricksFromTable(level)

        totalBricksAtStart = 0
        local activeBricksCount = 0
        for yi=1, #level.bricks do
            for xi=1, #level.bricks[yi] do
                -- create brick?
                if(level.bricks[yi][xi] &gt; 0) then
                    local xPos
                    local yPos
                    if(level.align == ""center"") then
                        --1100-((99*16)*0.5)
                        xPos = display.contentCenterX- ((level.columns * brickSize.width) * 0.5/3) + ((xi-1) * level.xSpace)--display.contentCenterX 
                        --xPos = 300 +(xi * level.xSpace)
                        yPos = 100 + (yi * level.ySpace)--100
                    else
                        xPos = level.xStart + (xi * level.xSpace)
                        yPos = level.yStart + (yi * level.ySpace)
                    end

                    local brickData = 
                    {
                        x = xPos,
                        y = yPos,
                        brickType = level.bricks[yi][xi],
                        index = activeBricksCount+1
                    }
                    bricks[activeBricksCount+1] = CreateBrick(brickData)

                    activeBricksCount = activeBricksCount + 1

                end

            end 

        end

        totalBricks = activeBricksCount
        totalBricksAtStart = activeBricksCount


    end

    -- create bricks for level --&gt; set from above functions, change function to change brick build type
    local CreateAllBricks = CreateBricksFromTable
    -- called by a timer so I can pass arguments to CreateAllBricks
    local function CreateAllBricksTimerCall()
        CreateAllBricks(Levels.currentLevel)
    end 
    -- remove all brick objects from memory
    local function ClearBricks()

        for i=1, #bricks do
            bricks[i] = nil
        end

    end
    -- stuff run on enterFrame event
    function Bricks:Update()

    -- update individual bricks
        if(totalBricksAtStart &gt; 0) then
            for i=1, totalBricksAtStart do
                -- brick exists?
                if(bricks[i]) then
                    bricks[i]:Update()
                end 
            end 
        end
        -- is level over?
        if(totalBricksBroken == totalBricks) then
            Events.allBricksBroken:Dispatch()
        end

    end
    ----------------
    -- Events
    ----------------
    function Bricks:allBricksBroken(event)
        -- cleanup bricks
        ClearBricks()
        local t = timer.performWithDelay( 1000, CreateAllBricksTimerCall)
        --CreateAllBricks()
        totalBricksBroken = 0       

        -- play happy sound for player to enjoy                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
        sound.play(sound.win)

        print(""You Win!"")
    end
    Events.allBricksBroken:AddObject(Bricks)
    CreateAllBricks(Levels.currentLevel)
    return Bricks
    </code></pre>

    <p>And Levels.lua</p>

    <pre><code>local Events = require(""Events"")
    local Levels = {}
    local function MakeLevel(data)
        local level = {}
        level.xStart = data.xStart or 100
        level.yStart = data.yStart or 100
        level.xSpace = data.xSpace or 23
        level.ySpace = data.ySpace or 23
        level.align = data.align or ""center""
        level.columns = data.columns or #data.bricks[1]
        level.bricks = data.bricks --&gt; required
        return level
    end
    Levels.test4 = MakeLevel
    {
        bricks =
        {
            {0,2,0,0,2,0,0,2,0},
            {0,0,2,0,2,0,2,0,0},
            {0,0,0,0,2,0,0,0,0},
            {1,1,2,1,1,1,2,1,1},
            {0,0,0,0,1,0,0,0,0},
            {0,0,0,0,1,0,0,0,0},
            {0,0,0,0,1,0,0,0,0},
        }
    }

    Levels.test5 = MakeLevel
    {
        bricks =
        {       
                        {0,0,0,1,0,0,0,0},
                         {0,0,1,0,1,0,0,0},
                         {0,0,1,0,1,0,0,0},
                         {0,1,0,0,0,1,0,0},
                         {0,1,1,1,1,1,0,0},
                         {1,0,0,0,0,0,1,0},
                         {1,0,0,0,0,0,1,0},
                         {1,0,0,0,0,0,1,0},
                         {1,0,0,0,0,0,1,0}
        }
    }
    -- Levels.test6 = MakeLevel2
    -- {
    --  bricks =
    --  {
    ----A         ""a"" = {{0,0,0,1,0,0,0,0},
    --                   {0,0,1,0,1,0,0,0},
    --                   {0,0,1,0,1,0,0,0},
    --                   {0,1,0,0,0,1,0,0},
    --                   {0,1,1,1,1,1,0,0},
    --                   {1,0,0,0,0,0,1,0},
    --                   {1,0,0,0,0,0,1,0},
    --                   {1,0,0,0,0,0,1,0},
    --                   {1,0,0,0,0,0,1,0}},
    ----B
    --            ""b"" = {{1,1,1,1,0,0,0},
    --                   {1,0,0,0,1,0,0},
    --                   {1,0,0,0,1,0,0},
    --                   {1,0,0,0,1,0,0},
    --                   {1,1,1,1,0,0,0},
    --                   {1,0,0,0,1,0,0},
    --                   {1,0,0,0,0,1,0},
    --                   {1,0,0,0,0,1,0},
    --                   {1,1,1,1,1,0,0}},
    --...........
    --.......
    --...
    -- --Z
    --             ""z""= {{1,1,1,1,1,1,1,0},
    --                   {0,0,0,0,0,1,0,0},
    --                   {0,0,0,0,1,0,0,0},
    --                   {0,0,0,0,1,0,0,0},
    --                   {0,0,0,1,0,0,0,0},
    --                   {0,0,1,0,0,0,0,0},
    --                   {0,0,1,0,0,0,0,0},
    --                   {0,1,0,0,0,0,0,0},
    --                   {1,1,1,1,1,1,1,0}} 
    --  }
    -- }
    -- stores all levels in ordered table so that one can be selected randomly by index
    Levels.levels = 
    {
        --Levels.test4,
         Levels.test5
        -- Levels.test6,
    }
    function Levels:GetRandomLevel()
        return self.levels[math.random(#Levels.levels)]
    end
    Levels.notPlayedYet = {}
    Levels.currentLevel = Levels:GetRandomLevel()
    -- Events
    function Levels:allBricksBroken(event)
        self.currentLevel = Levels:GetRandomLevel()
    end
    Events.allBricksBroken:AddObject(Levels)
    return Levels
    </code></pre>
    ";

        public TrimTests()
        {
            var chars = new List<char>();
            for (int i = char.MinValue; i <= char.MaxValue; i++)
            {
                var c = Convert.ToChar(i);
                if (char.IsWhiteSpace(c))
                {
                    chars.Add(c);
                }
            }
            chars.Add('\u200c');
            _whitespaceChars = chars.ToArray();
        }

        [Benchmark]
        public string StackTrimUnicode() => TestPost.TrimUnicode();

        [Benchmark]
        public string StringTrim() => TestPost.Trim();

        [Benchmark]
        public string StringTrimParams() => TestPost.Trim(_whitespaceChars);
    }

    public static class StringExtensions
    {
        public static string TrimUnicode(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var start = 0;
            var len = s.Length;
            while (start < len && (char.IsWhiteSpace(s[start]) || s[start] == '\u200c'))
                start++;
            var end = len - 1;
            while (end >= start && (char.IsWhiteSpace(s[end]) || s[end] == '\u200c'))
                end--;
            if (start >= len || end < start)
                return "";
            return s.Substring(start, end - start + 1);
        }
    }
}
