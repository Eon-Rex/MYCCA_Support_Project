var appMaster = {
animateScript: function(){
$('.asd h3').waypoint(function(){
$(this).toggleClass('animated fadeInRight');
},{offset:'100%'});

$('.icon img').waypoint(function(){
$(this).toggleClass('animated rollIn');
},{offset:'100%'});  

$('.header-menu').waypoint(function(){
$(this).toggleClass('animated fadeIn');
},{offset:'100%'});

$('.inner-layer h3').waypoint(function(){
$(this).toggleClass('animated fadeInLeft');
},{offset:'100%'});


$('.second-inner, right-img').waypoint(function(){
$(this).toggleClass('animated fadeInUp');
},{offset:'100%'});

$('.background img').waypoint(function(){
$(this).toggleClass('animated fadeInRight');
},{offset:'100%'});

$('#image-section h1').waypoint(function(){
$(this).toggleClass('animated fadeInUp');
},{offset:'100%'});

$('.third-inner h1').waypoint(function(){
$(this).toggleClass('animated rotateInDownLeft');
},{offset:'100%'});


$('.testimonial').waypoint(function(){
$(this).toggleClass('animated rotateInDownRight');
},{offset:'100%'});

$('.footer-start h1').waypoint(function(){
$(this).toggleClass('animated flipInY');
},{offset:'100%'});

$('.footer-matter a').waypoint(function(){
$(this).toggleClass('animated flipInY');
},{offset:'100%'});


$('#fade-in h2').waypoint(function(){
$(this).toggleClass('animated fadeInDown');
},{offset:'100%'});

$('#zoom-in h2').waypoint(function(){
$(this).toggleClass('animated fadeInDown');
},{offset:'100%'});

$('.effect4').waypoint(function(){
$(this).toggleClass('animated zoomIn');
},{offset:'100%'});

$('.effect5').waypoint(function(){
$(this).toggleClass('animated zoomInDown');
},{offset:'100%'});

$('.header-text h4').waypoint(function(){
$(this).toggleClass('animated fadeInUp');
},{offset:'100%'});

$('.header-text h5').waypoint(function(){
$(this).toggleClass('animated fadeInUp');
},{offset:'100%'});

$('.effect6').waypoint(function(){
$(this).toggleClass('animated rotateInUpLeft');
},{offset:'100%'});

$('.effect6 h4').waypoint(function(){
$(this).toggleClass('animated fadeInRight');
},{offset:'100%'});

$('.effect6 h5').waypoint(function(){
$(this).toggleClass('animated fadeInLeft');
},{offset:'100%'});

$('.effect7').waypoint(function(){
$(this).toggleClass('animated rotateInDownRight');
},{offset:'100%'});

$('.effect7 h4').waypoint(function(){
$(this).toggleClass('animated fadeInRight');
},{offset:'100%'});

$('.effect7 h5').waypoint(function(){
$(this).toggleClass('animated fadeInLeft');
},{offset:'100%'});








}
};





$(document).ready(function() {

var winWidth = $(window).width();
if(winWidth>767){
appMaster.animateScript();
}

});// JavaScript Document