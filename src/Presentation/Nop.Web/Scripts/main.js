$(document).ready(function() {

  var owl = $("#last-products-carousel");
 
  owl.owlCarousel({
     
      navigation : true,
      navigationText : ['', '']
 
  });

  owl = $("#popular-products-carousel");
 
  owl.owlCarousel({
     
      navigation : true,
      navigationText : ['', '']
 
  });

  $('.small-image img').click(function() { 
		$('.main-image img').attr('src', $(this).attr('src')); 
	});

	$('.rating').hover(function() {

	}, function() {

	});
 
 	$('.callback').click(function(e) {
 		e.stopPropagation();
 		$('#wrapper').css('opacity', '0.5');
 		$('#popup-wrapper').css('left', ($(window).width()-600)/2).show();
 	});
 	$('#wrapper').click(function() {
 		$('#wrapper').css('opacity', '1');
 		$('#popup-wrapper').hide();
 	});

 	$('.close').click(function() {
 		$('#wrapper').css('opacity', '1');
 		$('#popup-wrapper').hide();
 	});

 	$('#popup-wrapper .button').click(function() {
 		$('#popup-wrapper').html('<h1>Отправьте заявку</h1><img src="/img/close.png" class="close" /><h2>Заявка успешно подана. Мы вам перезвоним!</h2>');
 		$('.close').click(function() {
	 		$('#wrapper').css('opacity', '1');
	 		$('#popup-wrapper').hide();
	 	});
 	});

 	$('.feedback').click(function() {
 		$("html, body").animate({scrollTop:$('.feedbacks').offset().top}, '200', 'swing');
 	});
});