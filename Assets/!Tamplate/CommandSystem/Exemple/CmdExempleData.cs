using UnityEngine;
using System;

// Контекст бонуса
public class ExempleCmdContext : CommandContext
{
    public Vector3 hitPoint;
}

// Базовый класс для всех команд бонусов
public abstract class ExempleCmdBeh : CommandBehaviour<ExempleCmdContext> { }
