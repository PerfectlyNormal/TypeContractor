enum CleanMethod
{
    /// <summary>
    /// Leave everything as-is. Cleanup is left to the user
    /// </summary>
    None,

    /// <summary>
    /// Remove files and directories that are not used
    /// </summary>
    Smart,

    /// <summary>
    /// Remove the entire output directory and re-create from scratch
    /// </summary>
    Remove,
}
