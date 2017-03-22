alertify.defaults = {
	// dialogs defaults
	modal : true,
	movable : true,
	resizable : true,
	closable : true,
	maximizable : true,
	pinnable : true,
	pinned : true,
	padding : true,
	overflow : true,
	maintainFocus : true,
	transition : '',

	// notifier defaults
	notifier : {
		// auto-dismiss wait time (in seconds)
		delay : 5,
		// default position
		position : 'bottom-right'
	},

	// language resources
	glossary : {
		// dialogs default title
		title : 'ScanPack',
		// ok button text
		ok : 'Aceptar',
		// cancel button text
		cancel : 'Cancelar'
	},

	// theme settings
	theme : {
		// class name attached to prompt dialog input textbox.
		input : 'ajs-input',
		// class name attached to ok button
		ok : 'ajs-ok',
		// class name attached to cancel button
		cancel : 'ajs-cancel'
	}
};