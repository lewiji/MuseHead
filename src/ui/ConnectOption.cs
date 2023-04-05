using System;
using Godot;

namespace MuseHead.ui;

[Tool]
public partial class ConnectOption : PanelContainer
{
	[Export] public StringName OptionName {
		get => _optionName;
		set {
			_optionName = value;
			CallDeferred(MethodName.SetLabelText);
		}
	}
	StringName _optionName = new ("Option");

	[Export] public OptionType OptionType {
		get => _optionType;
		set {
			_optionType = value;
			CallDeferred(MethodName.SetOptionType);
		}
	}
	OptionType _optionType = OptionType.String;

	[Export] public StringName OptionDefault {
		get => _optionDefault;
		set {
			_optionDefault = value;
			CallDeferred(MethodName.SetOptionDefault);
		}
	}
	StringName _optionDefault = new();

	Label _label = default!;
	Control _optionContainer = default!;
	bool _changed;
		
	public override void _Ready()
	{
		_label = GetNode<Label>("%Label");
		_optionContainer = GetNode<Control>("%OptionContainer");
		SetLabelText();
		SetOptionType();
		SetOptionDefault();
	}

	void SetLabelText()
	{
		GD.Print(_optionName);
		_label.Text = $"{_optionName}:";
	}

	void SetOptionType()
	{
		Control control;

		switch (_optionType)
		{
			case OptionType.String:
				control = new LineEdit {Text = OptionDefault};
				break;
			case OptionType.Integer:
				control = new SpinBox {Value = int.Parse(OptionDefault), MaxValue = UInt16.MaxValue, MinValue = 0};
				break;
			default: throw new ArgumentOutOfRangeException();
		}

		if (_optionContainer.GetNodeOrNull("Input") is { } existingInput)
		{
			_optionContainer.RemoveChild(existingInput);
			existingInput.QueueFree();
		}

		control.Name = "Input";
		control.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		_optionContainer.AddChild(control);
	}

	void SetOptionDefault()
	{
		if (_optionContainer.GetNodeOrNull("Input") is not { } existingInput) return;

		switch (existingInput)
		{
			case LineEdit lineEdit: lineEdit.Text = _optionDefault;
				break;
			case SpinBox spinBox when int.TryParse(_optionDefault, out var intDefault): spinBox.Value = intDefault;
				break;
		}
	}

	public Variant GetValue()
	{
		return _optionType switch {
			OptionType.String => _optionContainer.GetNode<LineEdit>("Input").Text,
			OptionType.Integer => _optionContainer.GetNode<SpinBox>("Input").Value,
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}