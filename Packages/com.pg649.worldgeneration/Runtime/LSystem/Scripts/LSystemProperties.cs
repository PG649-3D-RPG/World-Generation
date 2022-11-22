namespace LSystem
{
    /// <summary>
    /// Struct for passing properties of the lsystem to another object.
    /// </summary>
    public struct LSystemProperties
    {
        public float distance;
        public short angle;
        public INITIAL_DIRECTION initialDirection;
        public float thickness;
        public uint crossSections;
        public uint crossSectionDivisions;

        public bool translatePoints;
        public string startString;
        public uint iterations;

        public string[] rules;

        public LSystemProperties(float distance, short angle, INITIAL_DIRECTION initialDirection, float thickness, uint crossSections, uint crossSectionDivisions, bool translatePoints, string startString, uint iterations, string[] rules)
        {
            this.distance = distance;
            this.angle = angle;
            this.initialDirection = initialDirection;
            this.thickness = thickness;
            this.crossSections = crossSections;
            this.crossSectionDivisions = crossSectionDivisions;
            this.translatePoints = translatePoints;
            this.startString = startString;
            this.iterations = iterations;
            this.rules = rules;
        }
    }
}